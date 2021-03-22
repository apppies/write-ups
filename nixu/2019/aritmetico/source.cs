using System;
using System.Text;
using System.Diagnostics;
using System.Security.Cryptography;

// Using nuget iExpr.Extensions.Math.Numerics for BigDecimals
using iExpr.Extensions.Math.Numerics;

namespace NixuOHO
{
    class Program
    {
        static void Main(string[] args)
        {
            var toDecrypt = Convert.FromBase64String("Q/aaG1P19kzJV7Szeg63oIUZs9gSo+0T3kYZ2Q8Qlj1NVUYzvoCJSUWI0bPRdr4p");

            var next = new BigDecimal(0, 100);
            for (int i = 1; i < 200; i++)
            {
                var newValue = getNext(i);
                Debug.WriteLine(newValue);
                next += newValue;
                Debug.WriteLine(next);
            }

            if (next - next.Integer >= 0.5)
                next++;
            var key = next.Integer.ToString();

            var sha256Hash = SHA256.Create();
            var sha256Bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(key));

            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                var dec = aes.CreateDecryptor(sha256Bytes, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

                byte[] decryptedBytes = dec.TransformFinalBlock(toDecrypt, 0, toDecrypt.Length);
                Debug.WriteLine($"Decrypted: {Encoding.UTF8.GetString(decryptedBytes)}");
            }
        }

        static BigDecimal getNext(int n)
        {
            var a = BigDecimal.Parse("58498716739671875697238968542");
            var d = BigDecimal.Parse("7868849012967");
            var b = BigDecimal.Parse("91957681313867825728");
            var o = a + d * (n - 1);
            var s = new BigDecimal(1, 100) / new BigDecimal(Math.Pow(2, (n - 1)), 100);

            return o * b * s;
        }
    }
}