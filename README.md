# GitDotNetClone

**GitDotNetClone** is a minimal version-control system written in C#, inspired by the internal architecture of Git. This project aims to demystify Git's core mechanisms like hashing, object storage, staging, and commits — implemented from scratch using idiomatic .NET code.

## 🚀 Features (In Progress)

- [x] `init` – Initialize a new repository (`.git` directory)
- [x] `add` – Track changes to files by hashing and storing them
- [ ] `commit` – Save a snapshot of the staged changes
- [ ] `status` – Show current repository state
- [ ] `log` – View commit history

## Project Main Components

- **Object Storage**: Store and retrieve Git objects (blobs) using SHA-256 hashes.
- **Index Management**: Add files and directories to a staging area (`.git/index`) with normalized paths.
- **Path Utilities**: Cross-platform path normalization and repository root detection.
- **Test Coverage**: xUnit-based tests for core storage and helper functionality.

## Project Structure

- `src/Core`: Core logic for object and index storage.
- `src/CrossCuttingConcerns`: Shared helpers (e.g., `PathHelper`).
- `test/`: Unit tests for all major components.

## Testing

- Uses xUnit for unit testing.
- Run all tests with `dotnet test`.
   
