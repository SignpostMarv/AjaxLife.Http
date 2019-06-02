using IpAddress = System.Net.IPAddress;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using System;
using X509Certificate2 = System.Security.Cryptography.X509Certificates.X509Certificate2;
using OpenFlags = System.Security.Cryptography.X509Certificates.OpenFlags;
using StoreName = System.Security.Cryptography.X509Certificates.StoreName;
using StoreLocation = System.Security.Cryptography.X509Certificates.StoreLocation;
using X509Store = System.Security.Cryptography.X509Certificates.X509Store;

/*
BouncyCastle code adapted from https://stackoverflow.com/questions/22230745/generate-self-signed-certificate-on-the-fly
*/

namespace AjaxLife.Http
{
    public class AjaxLifeHttpsServer : AjaxLifeHttpServer
    {
        public AjaxLifeHttpsServer(IpAddress addr, int port) : base(addr, port)
        {

        }

        public override bool IsSecure { get { return true; } }

        public override void Start(int backlog)
        {
            server.Start(Address, Port, GenerateCertificate());
        }

        protected static AsymmetricKeyParameter GenerateCA(
            string subjectName = "CN=Local AjaxLife CA",
            int strength = 2048,
            string hashAlgo = "SHA256WithRSA",
            double age = 86400
        )
        {
            CryptoApiRandomGenerator genRandom = new CryptoApiRandomGenerator();
            SecureRandom random = new SecureRandom(genRandom);
            X509V3CertificateGenerator genCert = new X509V3CertificateGenerator();

            BigInteger serial = BigIntegers.CreateRandomInRange(
                BigInteger.One,
                BigInteger.ValueOf(Int64.MaxValue),
                random
            );

            genCert.SetSerialNumber(serial);
            genCert.SetSignatureAlgorithm(hashAlgo);

            X509Name dn = new X509Name(subjectName);

            genCert.SetSubjectDN(dn);
            genCert.SetIssuerDN(dn);
            genCert.SetNotBefore(DateTime.Now.AddSeconds(-86400));
            genCert.SetNotAfter(DateTime.Now.AddSeconds(age));

            RsaKeyPairGenerator genRsaKeyPair = new RsaKeyPairGenerator();
            genRsaKeyPair.Init(new KeyGenerationParameters(random, strength));

            AsymmetricCipherKeyPair keypair = genRsaKeyPair.GenerateKeyPair();

            genCert.SetPublicKey(keypair.Public);

            X509Certificate cert = genCert.Generate(keypair.Private, random);

            X509Certificate2 x509 = new X509Certificate2(cert.GetEncoded());

            AddCertToStore(x509, StoreName.Root, StoreLocation.CurrentUser);

            return keypair.Private;
        }

        protected static X509Certificate2 GenerateCertificate(
            string subjectName = "CN=localhost",
            string issuerName = "CN=Local AjaxLife CA",
            int strength = 2048,
            string hashAlgo = "SHA256WithRSA",
            double age = 86400
        )
        {
            AsymmetricKeyParameter issuerPrivateKey = GenerateCA(issuerName, strength, hashAlgo, age);

            CryptoApiRandomGenerator genRandom = new CryptoApiRandomGenerator();
            SecureRandom random = new SecureRandom(genRandom);
            X509V3CertificateGenerator genCert = new X509V3CertificateGenerator();

            genCert.SetSerialNumber(
                BigIntegers.CreateRandomInRange(
                    BigInteger.One,
                    BigInteger.ValueOf(Int64.MaxValue),
                    random
                )
            );
            genCert.SetSignatureAlgorithm(hashAlgo);
            genCert.SetIssuerDN(new X509Name(issuerName));
            genCert.SetSubjectDN(new X509Name(subjectName));
            genCert.SetNotBefore(DateTime.Now.AddSeconds(-86400));
            genCert.SetNotAfter(DateTime.Now.AddSeconds(age));

            RsaKeyPairGenerator genKeypair = new RsaKeyPairGenerator();
            genKeypair.Init(
                new KeyGenerationParameters(random, strength)
            );

            AsymmetricCipherKeyPair keypair = genKeypair.GenerateKeyPair();

            genCert.SetPublicKey(keypair.Public);

            PrivateKeyInfo info = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keypair.Private);
            X509Certificate2 x509 = new X509Certificate2(genCert.Generate(issuerPrivateKey, random).GetEncoded());

            Asn1Sequence seq = (Asn1Sequence)Asn1Object.FromByteArray(info.ParsePrivateKey().GetDerEncoded());

            if (seq.Count != 9)
            {
                throw new PemException("Malformed sequence in RSA private key");
            }

            RsaPrivateKeyStructure rsa = new RsaPrivateKeyStructure(seq);
            RsaPrivateCrtKeyParameters rsaParams = new RsaPrivateCrtKeyParameters(
                rsa.Modulus,
                rsa.PublicExponent,
                rsa.PrivateExponent,
                rsa.Prime1,
                rsa.Prime2,
                rsa.Exponent1,
                rsa.Exponent2,
                rsa.Coefficient
            );

            x509.PrivateKey = DotNetUtilities.ToRSA(rsaParams);

            AddCertToStore(x509, StoreName.My, StoreLocation.CurrentUser);

            return x509;
        }

        protected static void AddCertToStore(X509Certificate2 cert, StoreName name, StoreLocation location)
        {
            X509Store store = new X509Store(name, location);
            store.Open(OpenFlags.ReadWrite);
            store.Add(cert);
            store.Close();
        }
    }
}
