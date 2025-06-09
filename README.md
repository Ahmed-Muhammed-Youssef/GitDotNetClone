# GitDotNetClone

**GitDotNetClone** is a minimal, educational version-control system built with C#. It is heavily inspired by Git’s internal architecture and provides hands-on insight into how core version control concepts—such as object storage, hashing, staging, and committing—work under the hood. This project is ideal for developers who want to deepen their understanding of Git or build their own Git-like tool.

---

## 🚀 Implemented Commands

- `init` – Initialize a new repository (`.git` directory)
- `add` – Hashes and stages file(s) by storing their content in the object store.
- `commit` – Records the current staged snapshot along with metadata like message, tree, and parent.
- `status` – Shows the current working directory state vs. index and last commit (HEAD).

---

## 🧠 Key Concepts & Architecture

### 🔹 Object Storage
- All files are stored as **blobs** in a `.git/objects` folder.
- SHA-256 is used to generate content-based hashes (unlike Git’s SHA-1).
- Objects are stored in a subdirectory based on the first two characters of the hash (e.g., `.git/objects/ab/cdef123...`).
- All git objects are stored in JSON format for simplicity.

### 🔹 Index (Staging Area)
- Tracks the current state of files added via `add`.
- Maintains file paths and associated content hashes.
- Stored in JSON format inside `.git/index`.

### 🔹 Commits
- Each commit points to a tree of file entries (a simplified Git tree object).
- Metadata such as commit message, parent hash, and tree hash are stored in the object store.
- HEAD points to the latest commit and is stored in `.git/HEAD` and `.git/refs/heads/main`.

### 🔹 Status
- Compares working directory → index → HEAD.
- Reports:
  - Modified files (not staged)
  - New/deleted/modified files staged for commit
  - Untracked files

---

## 🗂️ Project Structure

```
GitDotNetClone/
├── src/
│   ├── CLI/                         # Command-line interface (init, add, commit, status)
│   ├── Core/
│   │   ├── Index/                   # Index and IndexEntry management
│   │   ├── Objects/                 # All git objects (e.g., commits and blobs)
│   │   ├── Services/                # Git object and commit handling
│   │   └── Stores/                  # I/O abstractions for object/index/HEAD
│   └── CrossCuttingConcerns/        # Shared helpers like path normalization
├── test/
│   └── CLI.Tests/                   # xUnit-based tests for CLI commands
|   └── Core.Test/                   # xUnit-based tests for Core components
|   └── CrossCuttingConcerns.Test/   # xUnit-based tests for helpers
├── README.md
└── GitDotNetClone.sln
```

---


## 🧪 Testing

- Tests are written using [xUnit](https://xunit.net/).
- Integration-style tests use real file system paths for realism.
- Each command (`init`, `add`, `commit`, `status`) is tested independently and in end-to-end workflows.

### 🧾 Run Tests

```bash
dotnet test
```

---

## 📎 Requirements

- .NET 9.0 SDK or later
- Windows/Linux/macOS

---

## 🧑‍💻 Contributing

Want to extend GitDotNetClone? Contributions are welcome!
- Fork the repository
- Create a branch (`git checkout -b feature/my-feature`)
- Commit your changes and push
- Open a pull request!

---

## 📜 License

This project is licensed under the MIT License.
