//Work Time Automatic Control StopWatch use Google Spreadsheets to save your work information in the cloud.
//Copyright (C) 2013  Tomayly Dmitry
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.  If not, see http://www.gnu.org/licenses/.
//
//Google APIs Client Library for .NET license: http://www.apache.org/licenses/LICENSE-2.0 .

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace WorkTimeAutomaticControl
{
	/// <summary>
	/// Processes encryption and decryption of user data
	/// </summary>
	internal static class Cryptography
	{
		private const byte AES_TAKE_KEY_ELEMENT_NUMBER = 24;
		private const byte AES_TAKE_IV_ELEMENT_NUMBER = 16;
		private const byte AES_BLOCK_SIZE = 128;

		private static string GetUniqueID
		{
			get { return System.Security.Principal.WindowsIdentity.GetCurrent().Name; }
		}

		private enum AesKeySize
		{
			Aes128 = 128,
			Aes192 = 192,
			Aes256 = 256
		}

		private enum HashSum
		{
			Md5,
			Sha1,
			Sha256,
			Sha512
		}

		/// <summary>
		/// Get encrypted message
		/// </summary>
		internal static string GetCryptedMessage(string message)
		{
			return GetAESEncryptedMessage(message, GetNeededAmountOfBytes(GetUniqueID, AES_TAKE_KEY_ELEMENT_NUMBER, HashSum.Sha1),
												   GetNeededAmountOfBytes(GetUniqueID, AES_TAKE_IV_ELEMENT_NUMBER));
		}

		/// <summary>
		/// Get decrypted message
		/// </summary>
		internal static string GetDecryptedMessage(string message)
		{
			return GetAESDecryptedMessage(message, GetNeededAmountOfBytes(GetUniqueID, AES_TAKE_KEY_ELEMENT_NUMBER, HashSum.Sha1),
												   GetNeededAmountOfBytes(GetUniqueID, AES_TAKE_IV_ELEMENT_NUMBER));
		}

		/// <summary>
		/// Returns needed amount of bytes from data source
		/// </summary>
		private static byte[] GetNeededAmountOfBytes(string dataSource, byte amountOfBytes, HashSum expecteedHashSum = HashSum.Md5)
		{
			return new ASCIIEncoding().GetBytes(GetHashSum(dataSource, expecteedHashSum))
									  .Take(amountOfBytes)
			                          .ToArray();
		}

		/// <summary>
		/// Provide MD5, SHA1, SHA256 and SHA512 hash summing
		/// </summary>
		/// <exception cref="ArgumentException"></exception>
		private static string GetHashSum(string inputData, HashSum selectedHashSum)
		{
			if (string.IsNullOrEmpty(inputData))
				throw new ArgumentException("inputData mast have value");

			switch (selectedHashSum)
			{
				case HashSum.Md5:
					{
						using (var md5 = MD5.Create())
						{
							return md5.ComputeHash(Encoding.ASCII.GetBytes(inputData))
									  .Select(el => el.ToString("x2"))
									  .Aggregate((curEl, nextEl) => curEl + nextEl);
						}
					}
				case HashSum.Sha1:
					{
						using (var sha1 = SHA1.Create())
						{
							return sha1.ComputeHash(Encoding.ASCII.GetBytes(inputData))
									   .Select(el => el.ToString("x2"))
									   .Aggregate((curEl, nextEl) => curEl + nextEl);
						}
					}
				case HashSum.Sha256:
					{
						using (var sha256 = SHA256.Create())
						{
							return sha256.ComputeHash(Encoding.ASCII.GetBytes(inputData))
										 .Select(el => el.ToString("x2"))
										 .Aggregate((curEl, nextEl) => curEl + nextEl);
						}
					}
				case HashSum.Sha512:
					{
						using (var sha256 = SHA512.Create())
						{
							return sha256.ComputeHash(Encoding.ASCII.GetBytes(inputData))
										 .Select(el => el.ToString("x2"))
										 .Aggregate((curEl, nextEl) => curEl + nextEl);
						}
					}
				default:
					return string.Empty;
			}
		}

		/// <summary>
		/// Provides AES 128, 192 and 256 message encryption
		/// </summary>
		/// <exception cref="ArgumentException"></exception>
		private static string GetAESEncryptedMessage(string message, byte[] key, byte[] iv, int blockSize = AES_BLOCK_SIZE, AesKeySize keySize = AesKeySize.Aes192)
		{
			if (string.IsNullOrEmpty(message))
				return null;

			if ((key == null) || (key.Length <= 0))
				throw new ArgumentException("key cant be null or empty");

			if ((iv == null) || (iv.Length <= 0))
				throw new ArgumentException("iv cant be null or empty");

			using (var aes = new AesManaged())
			{
				aes.KeySize = (int)keySize;
				aes.BlockSize = blockSize;
				aes.Key = key;
				aes.IV = iv;
				aes.Padding = PaddingMode.Zeros;

				using (var encryptMemorySteam = new MemoryStream())
				{
					using (var encryptCrypoSteam = new CryptoStream(encryptMemorySteam, aes.CreateEncryptor(aes.Key, aes.IV), CryptoStreamMode.Write))
					{
						using (var encryptStreamWriter = new StreamWriter(encryptCrypoSteam))
						{
							encryptStreamWriter.Write(message);
						}
					}

					return encryptMemorySteam.ToArray().Aggregate(string.Empty, (agg, el) => agg + el.ToString("D3"));
				}
			}
		}

		/// <summary>
		/// Provides AES 128, 192 and 256 message decryption
		/// </summary>
		/// <exception cref="ArgumentException"></exception>
		private static string GetAESDecryptedMessage(string message, byte[] key, byte[] iv, int blockSize = AES_BLOCK_SIZE, AesKeySize keySize = AesKeySize.Aes192)
		{
			const sbyte oneByteMaxLength = 3;

			if ((message == null) || (message.Length <= 0))
				return string.Empty;

			if ((key == null) || (key.Length <= 0))
				throw new ArgumentException("key cant be null or empty");

			if ((iv == null) || (iv.Length <= 0))
				throw new ArgumentException("iv cant be null or empty");

			using (var aes = new AesManaged())
			{
				aes.KeySize = (int)keySize;
				aes.KeySize =
				aes.BlockSize = blockSize;
				aes.Key = key;
				aes.IV = iv;
				aes.Padding = PaddingMode.Zeros;

				var sourceDecryptedFileData = new List<byte>();

				for (var counter = 0; counter < message.Length; counter += oneByteMaxLength)
				{
					sourceDecryptedFileData.Add(byte.Parse(message.Substring(counter, oneByteMaxLength)));
				}

				using (var decryptMemorySteam = new MemoryStream(sourceDecryptedFileData.ToArray()))
				{
					using (var decryptCryptoSteam = new CryptoStream(decryptMemorySteam, aes.CreateDecryptor(aes.Key, aes.IV), CryptoStreamMode.Read))
					{
						using (var decryptSteam = new StreamReader(decryptCryptoSteam))
						{
							return decryptSteam.ReadToEnd().Trim(new[] { '\0' });
						}
					}
				}
			}
		}
	}
}
