# ApiTester

ApiTester is a small Avalonia desktop application for crafting and sending HTTP requests. It provides a simple UI for building requests, viewing responses, and persisting request data to JSON.

This README explains how to build and run the project and documents the Headers and Authentication UI features that were recently added.

## Features

- HTTP request builder (method, URL, body)
- Headers subtab:
  - Header list presented as a table with visible borders.
  - The first header row is reserved for `Content-Type` and cannot be deleted or renamed. Its value drives the Content-Type radio buttons and the `Other` option.
  - Add / Remove / Clear controls (Remove is disabled for the Content-Type row; Clear preserves Content-Type).
  - Basic duplicate-name validation (case-insensitive) — duplicates are shown above the grid.
  - Headers are persisted when you `Save` and restored with `Load`.
- Authentication subtab:
  - Supports `None`, `Basic`, `Bearer`, and `ApiKey` schemes.
  - Basic: enter username and password (password is entered into a TextBox with `PasswordChar="*"`). Password is not persisted.
  - Bearer: enter the token.
  - ApiKey: enter the key, choose whether it should be sent in the Header or Query, and configure the header/query parameter name (default `X-API-KEY` / `api_key`).
  - Authentication values are applied when the request is sent.
- Copy-as-curl:
  - A "Copy as curl" button builds a curl command representing the current request and copies it to the clipboard.
  - The app attempts to include headers, body, and authentication (Basic/Bearer/ApiKey) into the generated curl.
  - A transient status message "Copied curl to clipboard" appears for 3 seconds after a successful copy.
- Copy-as-PowerShell:
  - A "Copy as PowerShell" button builds a small PowerShell snippet using `Invoke-RestMethod` that contains a `$headers` hashtable, the request body as a here-string, and the constructed URI.
  - The snippet includes authentication (Basic/Bearer/ApiKey) in headers or query when appropriate.
  - The snippet is copied to the clipboard and a transient status message "Copied PowerShell snippet to clipboard" appears for 3 seconds.
- Content-Type: None
  - New option "None" allows you to omit the Content-Type header entirely. When selected the `Content-Type` row is removed from the headers table; choosing a content type re-inserts it.

## Where to look in the source

- Main UI: `Views/MainWindow.axaml`
- View model + behavior: `ViewModels/MainWindowViewModel.cs`
- Request builder (applies headers & auth): `Builders/MessageBuilder.cs`
- Models: `Models/HeaderEntry.cs`, `Models/SerializableHeader.cs`, `Models/HttpRequestPersistentDataModel.cs`
- Converters (used in the UI): `Converters/EqualityToBooleanConverter.cs`

## Build and run

Prerequisites:

- .NET SDK (the project targets .NET 8.0 in this repo)

Build in a terminal (in repo root):

```bash
dotnet build
```

Run the app:

```bash
dotnet run
```

You can also use the provided VS Code tasks (build/publish/watch) if you use VS Code.

## Usage notes

- Headers:
  - The headers table contains a `Content-Type` row when a content type is selected. Edit its *value* to change the content type. If you type a value that is not one of the built-in types (`application/json`, `application/xml`, `text/plain`) the UI will automatically select `Other` and populate the custom content-type textbox. Selecting `None` removes the `Content-Type` row so no Content-Type header is sent.
  - Use `Add Row` to append header rows. Select a row then `Remove` to delete it (the Content-Type row cannot be removed).
  - `Clear` removes all header rows except the `Content-Type` row.
  - Empty header rows are saved/loaded as provided; you can change this behavior if you prefer to filter them out on save.

- Authentication:
  - Choose a scheme, fill the fields and click `Send`.
  - Basic auth uses `Authorization: Basic <base64(user:pass)>`.
  - Bearer uses `Authorization: Bearer <token>`.
  - ApiKey by default adds a header named `X-API-KEY`; change the name in the `API Key` panel if you need a different header or query parameter name.

  ## Copying to PowerShell

  If you paste a curl command (the app's "Copy as curl" output) into PowerShell, note that PowerShell's `-Headers` parameter expects a hashtable / IDictionary rather than curl-style `-H` strings. If you prefer to run requests from PowerShell, you can convert the curl headers into a hashtable or use the `Invoke-RestMethod` approach shown below.

  Quick PowerShell example (equivalent to a curl with JSON + header):

  ```powershell
  $uri = 'https://example.com/api'
  $body = '{"name":"value"}'
  $headers = @{ 'Content-Type' = 'application/json'; 'X-API-KEY' = 'your_key' }
  Invoke-RestMethod -Uri $uri -Method POST -Headers $headers -Body $body
  ```

  The app currently copies curl text; let me know if you'd like a separate "Copy as PowerShell" action that emits an `Invoke-RestMethod` snippet directly.

## Security & persistence

- The app persists request configuration (URL, method, body, headers). The `BasicPassword` and other auth secrets are not persisted to the saved JSON by default in this implementation — avoid saving secrets into shared files.
- Password input uses a masked TextBox (`PasswordChar='*'`). This hides the content visually but the value is stored in memory within the ViewModel while the app is running.

## Troubleshooting

- If the UI doesn't reflect loaded content type, ensure the saved JSON contains a header with the name `Content-Type` (case-insensitive). The loader uses the Content-Type header (if any) to set the UI state.
- If a feature seems broken, run `dotnet build` and check for compiler errors. I validated the code builds on the current branch.

## Next steps / Improvements


If you'd like any of these implemented, tell me which one and I will add it.

Generated from the current workspace on October 19, 2025. Use this README as a quick reference while developing and testing the app.
Made with copilot

## License

This project is licensed under the MIT License — see the `LICENSE` file in the repository root for details.
