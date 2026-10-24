# Municipal Elections API

JSON API used by the **Municipal Elections Companion** iOS app (COMP4977). It runs inside the same ASP.NET Core app and database as the website, so anything an admin changes on the website shows up in the app.

| | Base URL |
|---|---|
| Azure | `https://mems-isaac-cali-bfgehyf7f0h0fbhm.westus3-01.azurewebsites.net/api` |
| Local | `http://localhost:5211/api` (`dotnet run --launch-profile http`) |

All JSON uses camelCase property names. Try every request in `Municipal-Elections-Management-System.http`.

## Endpoints

| Method | Path | Auth | Success | Errors |
|---|---|---|---|---|
| POST | `/api/auth/register` | – | 200 `{ token }` | 400 |
| POST | `/api/auth/login` | – | 200 `{ token }` | 400, 401 |
| GET | `/api/auth/me` | JWT | 200 `UserProfile` | 401 |
| GET | `/api/municipalities` | – | 200 `[Municipality]` | |
| GET | `/api/municipalities/{id}` | – | 200 `Municipality` | 404 |
| GET | `/api/positions?municipalityId=` | – | 200 `[Position]` | |
| GET | `/api/positions/{id}` | – | 200 `Position` | 404 |
| GET | `/api/candidates?municipalityId=&positionId=` | – | 200 `[Candidate]` | |
| GET | `/api/candidates/search?name=&municipalityId=&positionId=` | – | 200 `[Candidate]` | |
| GET | `/api/candidates/{id}` | – | 200 `Candidate` | 404 |
| GET | `/api/savedcandidates` | JWT | 200 `[SavedCandidate]` | 401 |
| POST | `/api/savedcandidates` | JWT | 201 `SavedCandidate` | 400, 401, 404, 409 |
| DELETE | `/api/savedcandidates/{savedCandidateId}` | JWT | 204 (no body) | 401, 404 |

All query parameters are optional. `name` is a case-insensitive "contains" match.

## Authentication

Register or log in to get a token, then send it on protected requests:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

Tokens last 7 days. A missing, invalid or expired token returns **401** with no body, so the app should treat any 401 as "log out and show the login screen".

### POST /api/auth/register

```json
{ "name": "Jane Voter", "email": "jane@example.com", "password": "Passw0rd!" }
```

Returns `{ "token": "..." }`, so the user is signed in straight away.

Validation:

- `name` is required, max 100 characters.
- `email` is required, must be a valid email and not already registered.
- `password` must have at least 6 characters, with an uppercase letter, a lowercase letter, a digit and a symbol (ASP.NET Identity's rules).

### POST /api/auth/login

```json
{ "email": "jane@example.com", "password": "Passw0rd!" }
```

Returns `{ "token": "..." }`. A wrong email or password returns 401 `"Invalid email or password."`. After 5 wrong passwords the account is locked for 5 minutes.

The seeded admins can log in too: `super@elections.local` / `P@$$w0rd`.

### GET /api/auth/me

```json
{ "id": "547448f4-...", "name": "Jane Voter", "email": "jane@example.com" }
```

The token payload also has `sub` (id), `email` and `name` if you'd rather decode it.

## Response shapes

### Municipality

```json
{
  "municipalityId": 1,
  "name": "Pitt Meadows",
  "province": "British Columbia",
  "electionDate": "2026-10-17",
  "description": "A small city on the north bank of the Fraser River...",
  "logoUrl": "https://.../uploads/logos/3f1c....png",
  "positions": [ Position, ... ]
}
```

### Position

```json
{ "positionId": 1, "name": "Mayor", "seatsAvailable": 1, "municipalityId": 1, "municipalityName": "Pitt Meadows" }
```

### Candidate

```json
{
  "candidateId": 11,
  "name": "Laura Chen",
  "biography": "Laura Chen is running for School Trustee in Pitt Meadows...",
  "photoUrl": null,
  "website": null,
  "email": "laura.chen@example.com",
  "positionId": 3,
  "positionName": "School Trustee",
  "municipalityId": 1,
  "municipalityName": "Pitt Meadows"
}
```

### SavedCandidate

```json
{ "savedCandidateId": 1, "candidateId": 11, "dateSaved": "2026-10-22T09:07:55Z", "candidate": Candidate }
```

- `POST /api/savedcandidates` takes `{ "candidateId": 11 }`. It returns 404 if the candidate doesn't exist and 409 if it is already saved.
- `DELETE /api/savedcandidates/{id}` takes the **savedCandidateId**, not the candidateId. To show "Saved" on a candidate's details screen, look for that `candidateId` in the `GET /api/savedcandidates` list.

## Errors

Every error with a body is [ProblemDetails](https://www.rfc-editor.org/rfc/rfc9457) JSON. Show `errors` when it is present, otherwise show `title`:

```json
{ "title": "This candidate is already in your voting guide.", "status": 409 }
```

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { "Password": ["Passwords must have at least one digit ('0'-'9')."] }
}
```

Keys in `errors` are the request property names in PascalCase (`Name`, `Email`, `Password`, `CandidateId`).

## Notes for the Swift app

- **Dates:** `electionDate` is a plain date (`"2026-10-17"`). Decode it as a `String` and parse it with a `DateFormatter` using `"yyyy-MM-dd"`. Don't use `.iso8601` for it; that would show Oct 16 in Pacific time. `dateSaved` is a full UTC timestamp without fractional seconds, so `.iso8601` works for it.
- **Images:** `logoUrl` and `photoUrl` are full URLs, or `null` when there is no image (all seeded candidates). Use `AsyncImage` and show a placeholder for `null`.
- **Nullable fields:** `description`, `logoUrl`, `photoUrl`, `website` and `email` can be `null`. Make them optionals in the `Codable` structs.
- **Azure:** use the Azure base URL in the app. iOS blocks plain `http` (App Transport Security), so the local URL only works with an ATS exception.
