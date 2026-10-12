<img src="https://github.com/gordon-matt/Snippetz/blob/master/_Misc/Logo_Wide.jpg" alt="Logo" width="100%" />

Snippetz is a self-hosted place to keep code snippets. Each snippet has a title, description, language, code, and optional Markdown notes. You can pin one, tag it, and find it again from the header search or the tag list in the sidebar.

Snippets belong to the signed-in user. A language is also stored as a lowercase tag, so filtering by language and by tag uses the same list.

## Search

The header search understands two filters, plus free text:

```
card lang:csharp tags:ui,files
```

- `lang:` limits the search to those languages. Names are letters only, separated by commas.
- `tags:` does the same for tags.
- Anything else is matched against the title and description.

An empty search returns nothing. The sidebar tag list is a separate filter: **All snippets**, or one tag at a time.

## Raw code

Each snippet has a plain-text URL you can paste into a script:

```
GET /api/snippets/raw/{id}
```

The response is `text/plain` and requires the same sign-in as the rest of the app. You can copy that URL from the snippet page.

## Run it

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet run --project Snippetz
```

The development profile listens on `http://localhost:5288` (and `https://localhost:7288` for the HTTPS profile).

The default configuration uses SQLite (`snippetz.db` next to the web project) and ASP.NET Identity. On first start the app applies migrations and seeds an administrator:

| | |
| --- | --- |
| Username | `admin` |
| Email | `admin@snippetz.local` |
| Password | `Admin@123` |

Change that password before anyone else can reach the app. You can also sign up; new accounts get the User role.

## Authentication

Set `Authentication:Provider` in `appsettings.json`, or `Authentication__Provider` in the environment.

| Provider | What it does |
| --- | --- |
| `Identity` | Built-in accounts. Sign in with a username or an email. |
| `Keycloak` | Cookie session plus OpenID Connect. Set `Authentication:Keycloak` (`Authority`, `ClientId`, `ClientSecret`, and optionally `PublicOrigin` and `BackchannelBaseUrl`). |
| `None` | No sign-in. Everyone is a single local administrator. Useful on a machine only you use. |

Sign out is a POST to `/auth/logout`.

Administrators can open the log dashboard at `/logvue`.

## Database

Set `Database:Provider` and `ConnectionStrings:DefaultConnection`.

| Provider | Connection string |
| --- | --- |
| `Sqlite` | `Data Source=snippetz.db` |
| `SqlServer` | `Server=host;Database=snippetz;User Id=sa;Password=...;TrustServerCertificate=True` |
| `Npgsql` | `Host=host;Port=5432;Database=snippetz;Username=postgres;Password=...` |
| `MySql` | `Server=host;Database=snippetz;User=root;Password=...` |

An empty connection string falls back to an in-memory SQLite database named `SnippetzDb`. Tables live in the `app` schema, which SQLite ignores.

## Docker

`docker compose up -d` from the repository root starts the app and PostgreSQL.

- App: `http://localhost:8090`
- Postgres: port `5438`

The compose file sets `Database__Provider` to `Npgsql` and seeds the administrator from `ADMIN_PASSWORD` (default `Admin123!`). Copy `.env.example` to `.env` to change the passwords. A commented Keycloak service is included in `docker-compose.yml`.

## Credits

Snippetz is inspired by [snippet-box](https://github.com/pawelmalak/snippet-box) by Paweł Małak.

## Support

If you find this project useful, consider supporting its development.

<p align="center">
  <a href="https://buymeacoffee.com/vnmatt">
    <img src="https://github.com/gordon-matt/MyResume/blob/gh-pages/assets/images/BuyMeACoffee_Logo.png"
         alt="Buy Me a Coffee"
         height="40">
  </a>&nbsp;&nbsp;&nbsp;
  <a href="https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=gordon_matt%40live%2ecom&lc=AU&currency_code=AUD&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted">
    <img src="https://github.com/gordon-matt/MyResume/blob/gh-pages/assets/images/PayPal_Logo.png"
         alt="Donate with PayPal"
         height="40">
  </a>
</p>

<p align="center">
  Prefer using your phone? Scan the QR code:
</p>

<p align="center">
  <a href="https://buymeacoffee.com/vnmatt">
    <img src="https://github.com/gordon-matt/MyResume/blob/gh-pages/assets/images/BuyMeACoffee_QR.png"
         alt="Buy Me a Coffee QR Code"
         width="180">
  </a>
</p>

<img src="https://komarev.com/ghpvc/?username=gordon-matt&label=GitHub%20Hits%20Since%202025-06-01%3A%20&color=ff0000&style=flat" alt="gordon-matt" />