# PAW_labs

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
