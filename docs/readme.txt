COMP 4976 - Assignment 1

TEAM
- Cali Heung
- Isaac Kehler


CONTRIBUTIONS
Cali Heung
- Created the ASP.NET Core MVC project, the first Candidate, Municipality and
  Position models, added EF Core and switched the project to SQLite.
- Public website:
  - Municipalities page (each municipality with its positions and seats)
  - Candidates page with search by name and filter by municipality and position
  - Candidate details page
  - Home page and navigation bar
- Voting guide: add / view / remove candidates (saved in a cookie, no login),
  and the printable voting guide page.
- Admin area:
  - Dashboard with number of municipalities, positions and candidates
  - CRUD for municipalities (with logo upload), positions and candidates
    (with photo upload)
  - Image upload checks (type and size) on the client and the server
  - Super Admin / municipality admin checks on every admin page
- ERD (docs/ERD.drawio)

Isaac Kehler
- Final database design with EF Core Code First, the ApplicationDbContext,
  relationships, unique indexes and the InitialCreate migration.
- Seed data: municipalities, positions, candidates, the Super Admin role, one
  admin group per municipality and the seeded users (super, pm).
- ASP.NET Core Identity setup with roles, username login page, public
  registration turned off.
- MunicipalityAccessService that decides which municipalities an admin can
  manage.
- .gitignore (appsettings.Development.json is not committed).
- GitHub Copilot prompt, instructions and skills files (.github folder).
- Dark mode (bonus feature).


WHAT HAS NOT BEEN COMPLETED
- All required features are completed.


BONUS FEATURES
- Dark mode: a sun/moon button in the navbar switches between light and dark
  themes on every page, including the admin area. The first visit follows the
  device's light/dark setting, and the choice is remembered in the browser
  (localStorage). The printable voting guide always prints in light mode.


MAJOR CHALLENGES
- Our branches split early on (dev and main had different models), so we had to
  move everything onto one main branch and redo the Candidate pages.
- Making sure a municipality admin can only manage their own municipality.
- Image uploads: checking the file type and size in both the browser and on the
  server, and saving files outside wwwroot on Azure so they are not lost when
  the app is redeployed.
- Searching by name in SQLite is case sensitive, so both sides are changed to
  lower case before comparing.


TESTING INSTRUCTIONS
1. Copy appsettings.Development.example.json to appsettings.Development.json
   (it has the connection string and a development Jwt:Key; the app won't
   start without a Jwt:Key of at least 32 characters)
2. Run:  dotnet run
   The database is created, migrated and seeded on startup.
3. Public site:
   - Municipalities, Candidates, candidate details
   - Click "Add to guide" on a few candidates, open "My Guide", then "Print guide"
4. Admin login: click "Admin Login" in the navbar.
   - super can create, edit and delete all municipalities, positions and candidates.
   - pm can only see and manage Pitt Meadows positions and candidates, and can
     edit the Pitt Meadows municipality but can't create or delete them.
5. Image uploads only accept .jpg, .jpeg, .png or .webp files up to 2 MB.
6. Mobile app API (COMP4977): the endpoints are listed in docs/api-endpoints.md
   and can be tried with Municipal-Elections-Management-System.http.
   Accounts registered through the API have no admin group, so they get
   "Access denied" if they sign in to the website's Admin area.


DEPLOYED APPLICATION ON AZURE
URL: https://mems-isaac-cali-bfgehyf7f0h0fbhm.westus3-01.azurewebsites.net/


GITHUB REPOSITORY
https://github.com/calih33/Municipal-Elections-Management-System
