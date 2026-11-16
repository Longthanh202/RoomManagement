using Konscious.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Web.Helpers
{
    public static class PasswordHasher
    {
        private const int MemorySize = 65536;
        private const int Iterations = 3;
        private const int DegreeOfParallelism = 2;
        private const int HashSize = 32;
        private const int SaltSize = 16;

        public static string Hash(string password)
        {
            byte[] salt = new byte[SaltSize];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            using (var argon2 = new Argon2id(passwordBytes))
            {
                argon2.Salt = salt;
                argon2.MemorySize = MemorySize;
                argon2.Iterations = Iterations;
                argon2.DegreeOfParallelism =
                    DegreeOfParallelism;

                byte[] hash = argon2.GetBytes(HashSize);

                return string.Format(
                    "argon2id$v=19$m={0},t={1},p={2}${3}${4}",
                    MemorySize,
                    Iterations,
                    DegreeOfParallelism,
                    Convert.ToBase64String(salt),
                    Convert.ToBase64String(hash)
                );
            }
        }

        public static bool Verify(
            string password,
            string encodedHash)
        {
            try
            {
                string[] parts = encodedHash.Split('$');

                if (parts.Length != 5 ||
                    parts[0] != "argon2id" ||
                    parts[1] != "v=19")
                {
                    return false;
                }

                string[] parameters = parts[2].Split(',');

                int memory = int.Parse(
                    parameters[0].Split('=')[1]);

                int iterations = int.Parse(
                    parameters[1].Split('=')[1]);

                int parallelism = int.Parse(
                    parameters[2].Split('=')[1]);

                byte[] salt =
                    Convert.FromBase64String(parts[3]);

                byte[] expectedHash =
                    Convert.FromBase64String(parts[4]);

                byte[] passwordBytes =
                    Encoding.UTF8.GetBytes(password);

                using (var argon2 =
                    new Argon2id(passwordBytes))
                {
                    argon2.Salt = salt;
                    argon2.MemorySize = memory;
                    argon2.Iterations = iterations;
                    argon2.DegreeOfParallelism = parallelism;

                    byte[] actualHash =
                        argon2.GetBytes(expectedHash.Length);

                    return FixedTimeEquals(
                        actualHash,
                        expectedHash);
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool FixedTimeEquals(
            byte[] a,
            byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;

            int diff = 0;

            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }

            return diff == 0;
        }
    }
}