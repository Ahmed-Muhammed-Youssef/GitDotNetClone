﻿using CLI.Commands;
using CLI.Test.Fakes;
using Core.Services;
using Core.Stores;
using System.Text.Json;

namespace CLI.Test.Commands
{
    public class AddCommandTests : IDisposable
    {
        private readonly string _tempRoot;
        private readonly IndexStore _indexStore;
        private readonly JsonSerializerOptions _jsonOptions;

        public AddCommandTests()
        {
            _tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempRoot);

            Directory.CreateDirectory(Path.Combine(_tempRoot, ".git")); // Simulate Git repo

            _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            _indexStore = new IndexStore(_tempRoot, _jsonOptions);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempRoot))
                Directory.Delete(_tempRoot, recursive: true);
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task ExecuteAsync_AddsSingleFileToIndex()
        {
            // Arrange
            IGitContextProvider _gitContextProvider = new FakeGitContextProvider(_tempRoot, true, _tempRoot);
            string filePath = Path.Combine(_tempRoot, "test.txt");
            await File.WriteAllTextAsync(filePath, "hello");


            var command = new AddCommand(_indexStore, ["test.txt"], _gitContextProvider);

            // Act
            await command.ExecuteAsync();

            // Assert
            var entries = _indexStore.GetEntries();
            Assert.Single(entries);
            Assert.EndsWith("test.txt", entries[0].FilePath);
        }

        [Fact]
        public async Task ExecuteAsync_AddsDirectoryToIndex()
        {
            // Arrange
            IGitContextProvider _gitContextProvider = new FakeGitContextProvider(_tempRoot, true, _tempRoot);
            string dirPath = Path.Combine(_tempRoot, "folder");
            Directory.CreateDirectory(dirPath);
            await File.WriteAllTextAsync(Path.Combine(dirPath, "file1.txt"), "one");
            await File.WriteAllTextAsync(Path.Combine(dirPath, "file2.txt"), "two");

            var command = new AddCommand(_indexStore, ["folder"], _gitContextProvider);

            // Act
            await command.ExecuteAsync();

            // Assert
            var entries = _indexStore.GetEntries();
            Assert.Equal(2, entries.Count);
        }

        [Fact]
        public async Task ExecuteAsync_AddsCurrentDirectoryWhenDotProvided()
        {
            // Arrange
            IGitContextProvider _gitContextProvider = new FakeGitContextProvider(_tempRoot, true, _tempRoot);
            await File.WriteAllTextAsync(Path.Combine(_tempRoot, "a.txt"), "A");
            await File.WriteAllTextAsync(Path.Combine(_tempRoot, "b.txt"), "B");

            var command = new AddCommand(_indexStore, ["."], _gitContextProvider);

            // Act
            await command.ExecuteAsync();

            // Assert
            var entries = _indexStore.GetEntries();
            Assert.Contains(entries, e => e.FilePath.EndsWith("a.txt"));
            Assert.Contains(entries, e => e.FilePath.EndsWith("b.txt"));
        }

        [Fact]
        public void Create_InvalidArgs_ReturnsNull()
        {
            // Arrange
            IGitContextProvider gitContextProvider = new FakeGitContextProvider(_tempRoot, true);

            // Act
            var result = AddCommand.Create([], gitContextProvider);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Create_ValidArgs_ReturnsCommand()
        {
            //Arrange
            IGitContextProvider gitContextProvider = new FakeGitContextProvider(_tempRoot, true);
            Directory.CreateDirectory(Path.Combine(_tempRoot, ".git")); // Ensure .git directory exists

            // Act
            var result = AddCommand.Create(["."], gitContextProvider);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<AddCommand>(result);
        }
    }
}
