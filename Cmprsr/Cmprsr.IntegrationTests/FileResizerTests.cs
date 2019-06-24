using Cmprsr;
using Cmprsr.Configuration;
using Cmprsr.IO;
using Cmprsr.Parallelization;
using Cmprsr.Segmentation;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace Tests.Integration
{
	public class FileResizerTests
	{
		private string testFileName = "Test";
		private string testArchiveName = "Test.zip";
		private string testDecompressedFileName = "Test(Decompressed)";

		private FileResizer fileResizer;

		[SetUp]
		public void Setup()
		{
			fileResizer = new FileResizer(new BlockByBlockStreamWriter(
				new SyncSegmentStreamWriter(),
				new DataSegmenter(),
				new ParallelProcessor(
					new AsyncProcessor()),
				new SegmentalCompressionConfiguration { ThreadsCount = 4, SegmentSize = 1024 }
			));
		}

		[Test]
		public void Compress_FileSizeReduced()
		{
			// Arrange
			GenerateTestFile(testFileName);

			long initialFileSize = 0;

			using (FileStream fileStream = new FileStream(testFileName, FileMode.Open))
			{
				initialFileSize = fileStream.Length;
			}

			// Act
			fileResizer.Compress(testFileName, testArchiveName);

			long archiveLength = 0;

			using (FileStream fileStream = new FileStream(testArchiveName, FileMode.Open))
			{
				archiveLength = fileStream.Length;
			}
			// Assert
			Assert.True(archiveLength < initialFileSize);
		}

		[Test]
		public void Decompress_CreatesTheSameFileAsCompressedOne()
		{
			// Arrange
			GenerateTestFile(testFileName);

			// Act
			fileResizer.Compress(testFileName, testArchiveName);
			fileResizer.Decompress(testArchiveName, testDecompressedFileName);


			byte[] testFileBuffer;

			byte[] testDecompressedFileBuffer;

			using (FileStream testFileStream = new FileStream(testFileName, FileMode.Open))
			{
				using (FileStream testDecompressedFileStream = new FileStream(testDecompressedFileName, FileMode.Open))
				{
					testFileBuffer = new byte[testFileStream.Length];

					testDecompressedFileBuffer = new byte[testDecompressedFileStream.Length];
				}
			}

			// Assert
			Assert.AreEqual(testFileBuffer, testDecompressedFileBuffer);
		}

		[TearDown]
		public void TearDown()
		{
			if (File.Exists(testFileName)) File.Delete(testFileName);
			if (File.Exists(testArchiveName)) File.Delete(testArchiveName);
		}

		private byte[] GenerateBufferRandomly(long size)
		{
			Random rnd = new Random();

			byte[] buffer = new byte[size];

			rnd.NextBytes(buffer);

			return buffer;
		}

		private void GenerateTestFile(string fileName)
		{
			using (FileStream fileStream = new FileStream(fileName, FileMode.OpenOrCreate))
			{
				using (StreamWriter streamWriter = new StreamWriter(fileStream))
				{
					streamWriter.Write(string.Join(',', GenerateBufferRandomly(8195).Select(b => b.ToString())));
				}
			}
		}

		private void DeleteTestFile(string fileName)
		{
			File.Delete(fileName);
		}
	}
}