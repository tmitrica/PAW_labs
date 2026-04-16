# Lab06

## Video Youtube

https://www.youtube.com/watch?v=seRkYaHgy_E

## Structura proiectului

**Controllers:** Reprezinta punctul de intrare in aplicatie. Primesc request-urile HTTP de la utilizator, valideaza datele primite (ModelState), apeleaza metodele din Service Layer si decid ce View sa returneze sau unde sa faca redirect.
**Services (Business Logic Layer):** Aici traieste "creierul" aplicatiei. Preia datele de la Controller, aplica regulile de business (ex: setarea automata a datei de publicare a unui articol) si comunica mai departe cu Repositoriile.
**Repositories (Data Access Layer):** Se ocupa strict de interactiunea cu baza de date. Contin interogarile LINQ si ascund detaliile de implementare ale Entity Framework Core fata de restul aplicatiei.
**ViewModels:** Sunt clase simple folosite strict pentru a transfera date intre Controller si View. Ne ajuta sa nu expunem modelele noastre de baza (entitatile din baza de date) si sa trimitem catre interfata doar datele strict necesare (ex: `ArticleViewModel` contine `CategoryName` sub forma de string, in loc de intregul obiect `Category`).
**Views:** Reprezinta interfata grafica (UI). Fisierele `.cshtml` care combina HTML cu cod C# (Razor) pentru a afisa datele utilizatorului final.

## Parcurgerea unui flux complet

1. **View -> Controller:** Utilizatorul completeaza formularul in View (`Create.cshtml`) si apasa submit. Datele ajung la metoda `Create` (POST) din `ArticlesController` sub forma unui `CreateArticleViewModel`.
2. **Controller -> Service:** Controller-ul valideaza datele, mapeaza ViewModel-ul intr-o entitate `Article` si apeleaza `await _articleService.AddAsync(article)`.
3. **Service -> Repository:** In `ArticleService`, se aplica logica de business: se seteaza `article.PublishedAt = DateTime.Now;`. Apoi, serviciul apeleaza `await _unitOfWork.ArticleRepository.AddAsync(article)`.
4. **Repository -> DbContext:** `ArticleRepository` preia obiectul si il adauga in tracking-ul bazei de date folosind `_context.Articles.Add(article)`. Tot prin Unit of Work se apeleaza `_context.SaveChangesAsync()`.
5. **DbContext -> Database:** Entity Framework genereaza codul SQL (instructiunea `INSERT`) si salveaza efectiv datele pe serverul SQL.
6. **Controller -> View:** Controlul se intoarce in `ArticlesController`, care vede ca operatiunea a avut succes si face un `RedirectToAction(nameof(Index))`, trimitand utilizatorul inapoi la lista de articole.

## Raspunsul la intrebari

**De ce folosim Repository Pattern?**
Folosim Repository Pattern pentru a decupla logica de acces la date de restul aplicatiei.Asa putem sa centralizam interogarile catre baza de date. Daca in viitor vrem sa schimbam ORM-ul (sa renuntam la Entity Framework) sau pur si simplu sa modificam o interogare complexa, o facem intr-un singur loc, fara sa spargem codul din controllere sau servicii.

**Ce s-ar intampla daca apelam `_context` direct din controller?**
Daca am folosi `AppDbContext` direct in Controller, am incalca principiul responsabilitatii unice (Single Responsibility Principle). Controller-ul devine prea mare, ocupandu-se si de rute web, si de reguli de business, si de query-uri SQL.

**De ce avem un Service Layer separat? Ce logica ar ajunge in controller fara el?**
Service Layer-ul izoleaza logica de business. Fara el, in Controller ar ajunge operatiuni precum: setarea datelor implicite, calcule, verificari complexe de validare, sau apeluri catre alte servicii.

**De ce folosim interfete (`IArticleRepository`, `IArticleService`)? Dati un exemplu concret din cod.**
Interfetele ne permit sa aplicam principiul Dependency Injection (Inversarea Dependentelor). Componentele noastre depind de "contracte" (interfete), nu de implementari concrete, ceea ce face codul extrem de flexibil si usor de testat automat.
* **Exemplu concret:** In constructorul din `ArticlesController`, injectam interfata: `public ArticlesController(IArticleService articleService)`. Controller-ul nu stie cum isi face `ArticleService` treaba sau ce baza de date foloseste in spate; el stie doar ca exista o metoda `GetAllAsync()` pe care o poate apela.

**Cum va ajuta aceasta structura daca adaugati un API REST sau o aplicatie mobila pe acelasi proiect?**
Nu trebuie sa rescriem nimic din logica aplicatiei daca vrem sa lansam o aplicatie pe mobil. 
Vom crea pur si simplu un controller nou (ex: `ArticlesApiController`) care va injecta `IArticleService`. Singura diferenta va fi ca noul controller va returna date in format JSON, in timp ce `ArticlesController`-ul actual returneaza View-uri HTML. Baza de date, Repository-ul si regulile din Service raman complet neatinse.


# Lab07

## Video Youtube

https://www.youtube.com/watch?v=cSDin0z6SGw

## Raspunsul la intrebari

**1. De ce Logout este implementat ca `<form method="post">` si nu ca un link `<a href="/Auth/Logout">`?**
Daca logout-ul ar fi un simplu link (GET), un site malitios ar putea ascunde acel link intr-o imagine (`<img src=".../Logout">`), iar utilizatorul ar fi delogat fara voia lui. De asemenea, browserele pot face pre-fetching la link-uri (navigand in avans pe ele pentru a incarca pagina mai repede), ceea ce ar deloga utilizatorul accidental doar navigand pe site.

**2. De ce login-ul face doi pasi in loc de unul?**
In ASP.NET Core Identity, `UserName` si `Email` sunt doua proprietati distincte in baza de date. Metoda `PasswordSignInAsync` asteapta prin definitie parametrul `UserName`, nu `Email`. De aceea, intai interogam baza de date folosind email-ul oferit in formular (pasul 1) pentru a extrage exact `UserName`-ul acelui cont, pe care abia apoi il trimitem catre functia de login pentru a valida parola si a crea sesiunea (pasul 2).

**3. De ce nu este suficient sa ascunzi butoanele Edit/Delete in View?**
Daca nu punem `[Authorize]` in controller, un utilizator rau intentionat poate trimite un request HTTP direct catre URL-ul de Edit/Delete, modificand baza de date ignorand complet interfata grafica.

**4. Ce este middleware pipeline-ul in ASP.NET Core?**
Middleware pipeline-ul reprezinta un lant de componente prin care trece fiecare request HTTP intrat in aplicatie (si response-ul asociat) pentru a fi procesat secvential.

**5. Ce am fi trebuit sa implementam manual daca nu foloseam ASP.NET Core Identity?**
Fara Identity, ar fi trebuit sa scriem de la zero:
- Tabelele si modelele pentru utilizatori si roluri din baza de date.
- Algoritmi de criptare pentru parole (salvarea parolelor in plain-text fiind o vulnerabilitate critica).
- Logica de generare, criptare, validare si stergere a cookie-urilor de sesiune la fiecare request.
- Sisteme de tip Lockout, confirmarea email-urilor si resetarea parolei prin token-uri unice.

**6. Care sunt dezavantajele folosirii ASP.NET Core Identity?**

- Genereaza o schema fixa cu multe tabele in baza de date, care poate fi foarte greu de modificat sau de integrat intr-un proiect cu o baza de date preexistenta complet diferita.
- Este construit in primul rand pentru aplicatii web traditionale (MVC/Razor Pages) care folosesc sistemul clasic de Cookies.
- Daca doresti sa expui un API consumat de o aplicatie de mobil (Android/iOS) sau de un framework frontend tip Angular, autentificarea pe baza de Cookies devine problematica.
