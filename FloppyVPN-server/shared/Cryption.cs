using System.Security.Cryptography;
using System.Text;

namespace FloppyVPN
{
	public static class Cryption
	{
		public static string Hash(string s)
		{
			string key = Config.Get("master_key");
			s += key; //add key as salt to maximally reduce bruteforce possibility

			using (SHA512 sha = SHA512.Create())
			{
				byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(s));

				StringBuilder builder = new();
				for (int i = 0; i < bytes.Length; i++)
				{
					builder.Append(bytes[i].ToString("x2"));
				}
				return builder.ToString();
			}
		}

		public static string GenerateRandomString(ushort length, string? dictionary = null)
		{
			if (string.IsNullOrEmpty(dictionary))
				dictionary = "qazxswedcrftgyhvbnujikolmp1092384756";

			char[] dic = dictionary.ToCharArray();

			const int REPETITIONS = 1_000_000;
			const int KEY_SIZE = 32;

			double expectedPercentage = 100.0 / dic.Length;

			Dictionary<char, long> grandTotalCounts = new();

			foreach (char ch in dic)
				grandTotalCounts.Add(ch, 0);

			for (int i = 0; i < REPETITIONS; i++)
			{
				var key = GetUniqueKey(KEY_SIZE, dic);

				foreach (var ch in key)
					grandTotalCounts[ch]++;
			}

			long totalChars = grandTotalCounts.Values.Sum();

			StringBuilder result = new StringBuilder(length);
			for (int i = 0; i < length; i++)
			{
				double rnd = GetNextRandomDouble();
				int idx = (int)Math.Floor(rnd * dic.Length);

				result.Append(dic[idx]);
			}

			return result.ToString();

			double GetNextRandomDouble()
			{
				byte[] buffer = new byte[4];
				using (var rng = RandomNumberGenerator.Create())
				{
					rng.GetBytes(buffer);
				}

				uint rand = BitConverter.ToUInt32(buffer, 0);
				return rand / (uint.MaxValue + 1.0);
			}

			string GetUniqueKey(int size, char[] dic)
			{
				byte[] data = new byte[4 * size];
				using (var crypto = RandomNumberGenerator.Create())
				{
					crypto.GetBytes(data);
				}
				StringBuilder result = new(size);
				for (int i = 0; i < size; i++)
				{
					uint rnd = BitConverter.ToUInt32(data, i * 4);
					long idx = rnd % dic.Length;

					result.Append(dic[idx]);
				}

				return result.ToString();
			}
		}

		// This constant is used to determine the keysize of the encryption algorithm in bits.
		// We divide this by 8 within the code below to get the equivalent number of bytes.
		private const int Keysize = 128;

		// This constant determines the number of iterations for the password bytes generation function.
		private const int DerivationIterations = 1000;

		public static string En(string plainText, string passPhrase)
		{
			// Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
			// so that the same Salt and IV values can be used when decrypting.  
			var saltStringBytes = Generate128BitsOfRandomEntropy();
			var ivStringBytes = Generate128BitsOfRandomEntropy();
			var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
			using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
			{
				var keyBytes = password.GetBytes(Keysize / 8);
				using (var symmetricKey = new RijndaelManaged())
				{
					symmetricKey.BlockSize = 128;
					symmetricKey.Mode = CipherMode.CBC;
					symmetricKey.Padding = PaddingMode.PKCS7;
					using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
					{
						using (var memoryStream = new MemoryStream())
						{
							using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
							{
								cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
								cryptoStream.FlushFinalBlock();
								// Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
								var cipherTextBytes = saltStringBytes;
								cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
								cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
								memoryStream.Close();
								cryptoStream.Close();
								return Convert.ToBase64String(cipherTextBytes);
							}
						}
					}
				}
			}
		}

		public static string De(string cipherText, string passPhrase)
		{
			// Get the complete stream of bytes that represent:
			// [32 bytes of Salt] + [16 bytes of IV] + [n bytes of CipherText]
			var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
			// Get the saltbytes by extracting the first 16 bytes from the supplied cipherText bytes.
			var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
			// Get the IV bytes by extracting the next 16 bytes from the supplied cipherText bytes.
			var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
			// Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
			var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

			using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
			{
				var keyBytes = password.GetBytes(Keysize / 8);
				using (var symmetricKey = new RijndaelManaged())
				{
					symmetricKey.BlockSize = 128;
					symmetricKey.Mode = CipherMode.CBC;
					symmetricKey.Padding = PaddingMode.PKCS7;
					using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
					{
						using (var memoryStream = new MemoryStream(cipherTextBytes))
						{
							using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
							{
								var plainTextBytes = new byte[cipherTextBytes.Length];
								var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
								memoryStream.Close();
								cryptoStream.Close();
								return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
							}
						}
					}
				}
			}
		}

		private static byte[] Generate128BitsOfRandomEntropy()
		{
			var randomBytes = new byte[16]; // 16 Bytes will give us 128 bits.
			using (var rngCsp = new RNGCryptoServiceProvider())
			{
				// Fill the array with cryptographically secure random bytes.
				rngCsp.GetBytes(randomBytes);
			}
			return randomBytes;
		}
	}
}