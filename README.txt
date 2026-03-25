CtxSignTool.NextSource

This source tree is a refactored CLI layout built around the current working model:
- params remain stable
- --sign --manifest builds/updates then signs a manifest
- --verify --manifest verifies the signed manifest, and if --in is a file under --root,
  also verifies that file against the manifest hash entry
- --makecert prints pin information after creation
- --printpins supports --out <file>

Notes:
- This source expects CtxSignlib and its Functions helpers to be available to the project.
- The manifest single-file verification path is intentionally implemented at the CLI layer by
  verifying the signed manifest first and then comparing the current file hash to the manifest entry.
- Localization starts with a single file: Localization/Lang/en.json
