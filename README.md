## מערכת VendingMachine – תרגיל בית

פתרון זה מממש מערכת Backend לניהול מכונות מכירה (Vending Machines) והמלאי שלהן, באמצעות .NET 8 Web API ובמבנה של **Clean Architecture**.

### מבנה הפתרון (Projects / Layers)

  שכבת ה‑Domain – ישויות וחוקי העסק:

- **VendingMachine.Domain**  
  - `VendingMachine` (Aggregate Root)
  - `Shelf`
  - `Product`
  - `ProductCategory`
  - חריגות דומיין:
    - `DomainException`
    - `MaxShelvesReachedException`
    - `InvalidProductCategoryException`
    - `ShelfCapacityExceededException`
    - `ProductNotFoundException`
    - `OutOfStockException`



- **VendingMachine.Application**
    **שכבת האפליקציה (Use Cases):**

   א.  DTOs: `VendingMachineDto`, `ShelfDto`, `ProductDto`
   ב. `IVendingMachineRepository` – ממשק לריפוזיטורי (בלי תלות בהטמעה)
   ג. `IVendingMachineService` + `VendingMachineService` – מימוש מקרי השימוש:
    - יצירת מכונת מכירה
    - הוספת מדף
    - טעינת מלאי
    - רכישת מוצר

- **VendingMachine.Infrastructure**  
  **שכבת התשתית (Infrastructure):**

  - אין בסיס נתונים אמיתי, אין EF Core, ואין תלות חיצונית.
  - מימוש In‑Memory של `IVendingMachineRepository` עם `ConcurrentDictionary<Guid, VendingMachine>`.



- **VendingMachine.Api**  
  **שכבת ה‑Web API (ASP.NET Core):**
  
   א. `VendingMachinesController` – Controller , שכל האחריות העסקית עוברת אליו דרך `IVendingMachineService`.:

   ב. Middleware גלובלי לניהול שגיאות: `ExceptionHandlingMiddleware`, שמחזיר פורמט אחיד:
    ```json
    {
      "code": "ERROR_CODE",
      "message": "Readable message"
    }
    ```

   ג. נקודות קצה (Endpoints):
    - `POST /api/machines` – יצירת מכונה
    - `GET  /api/machines/{machineId}` – שליפת מכונה
    - `POST /api/machines/{machineId}/shelves` – הוספת מדף
    - `POST /api/machines/{machineId}/shelves/{shelfId}/inventory` – טעינת מלאי למדף
    - `POST /api/machines/{machineId}/purchase` – רכישת מוצר

   ד.  Swagger / OpenAPI מופעל בדיבוג.

  **פרויקט טסטים (xUnit) עבור שכבת ה‑Domain בלבד.**


- **VendingMachine.Tests**  

### איך מריצים

1. **שחזור תלויות, Build והרצת טסטים**

   ```bash
   dotnet restore
   dotnet build
   dotnet test
   ```

2. **הרצת ה‑API**

   ```bash
   dotnet run --project VendingMachine.Api
   ```

3. **פתיחת Swagger UI**

   - ה‑API מאזין ל‑URL ולפורט המוגדרים ב‑`VendingMachine.Api/Properties/launchSettings.json` (לדוגמה `http://localhost:5088`).
   - כתובת Swagger (בדוגמה לעיל):
     - `http://localhost:5088/swagger`

> אם מתקבלת שגיאה שהפורט כבר בשימוש (address already in use), יש לעצור תהליך קודם (Ctrl+C בחלון ה‑terminal שבו הוא רץ), או לשנות את הפורט בקובץ `launchSettings.json`.

### כיסוי תרחישי הדוגמה (1–6)

ה‑Domain וה‑API תוכננו כך שיכסו את התרחישים מהתרגיל. יש גם טסט End‑to‑End (`EndToEnd_Scenarios_1_To_6`) בשכבת הטסטים שבודק אותם לוגית.

1. **יצירת מכונת מכירה שתומכת עד 5 מדפים**
   א. API: `POST /api/machines`
     ```json
     {
       "name": "Main Lobby VM",
       "location": "Lobby",
       "maxShelves": 5
     }
     ```
   ב. Domain: קריאה ל‑`VendingMachineService.CreateVendingMachineAsync` שיוצר `VendingMachine` עם `MaxShelves = 5`.

2. **הוספת 3 מדפים: שתייה 10, חטיפים 20, שתייה 15**
   - נניח שני סוגי קטגוריות (מזוהים לפי Guid):
     - Drinks: `drinkCategoryId`
     - Snacks: `snackCategoryId`
   - API: `POST /api/machines/{machineId}/shelves` :
     - מדף שתייה 1 (קיבולת 10):
       ```json
       { "productCategoryId": "<drinkCategoryId>", "capacity": 10 }
       ```
     - מדף חטיפים (קיבולת 20):
       ```json
       { "productCategoryId": "<snackCategoryId>", "capacity": 20 }
       ```
     - מדף שתייה 2 (קיבולת 15):
       ```json
       { "productCategoryId": "<drinkCategoryId>", "capacity": 15 }
       ```
   - Domain: `VendingMachine.AddShelf` מוסיף מדפים עד `MaxShelves` ומוודא שלא חורגים ממנו.

3. **טעינת 5 בקבוקי מים על מדף השתייה הראשון**
   א.  API: `POST /api/machines/{machineId}/shelves/{shelfId}/inventory`
     ```json
     {
       "productId": "<waterProductId>",
       "productName": "Water Bottle",
       "productCategoryId": "<drinkCategoryId>",
       "quantity": 5
     }
     ```
   ב. Domain: `VendingMachine.LoadInventory` → `Shelf.LoadProduct` מעדכן מלאי ומוודא קיבולת.

4. **ניסיון לטעון צ'יפס על מדף שתייה → כישלון**
   א. אותו Endpoint כמו בסעיף 3, אבל עם `productCategoryId = <snackCategoryId>`.
   ב. Domain: `Shelf.ValidateProductCategory` זורק `InvalidProductCategoryException` אם הקטגוריה של המוצר לא תואמת את `ProductCategoryId` של המדף.
   ג. API: ה‑Middleware ממפה את החריגה ל‑HTTP 400 עם קוד `INVALID_PRODUCT_CATEGORY`.

5. **ניסיון להוסיף מדף שישי → כישלון**
  א. אחרי הוספת 5 מדפים, עוד קריאה ל‑`POST /api/machines/{machineId}/shelves`:
  ב. Domain: `VendingMachine.AddShelf` בודק אם כמות המדפים הקיימת >= `MaxShelves` וזורק `MaxShelvesReachedException`.
  ג. API: ממופה ל‑HTTP 400 עם קוד `MAX_SHELVES_REACHED`.

7. **ניסיון לטעון 18 בקבוקי מים על מדף עם קיבולת 10 → כישלון**
   א. `quantity: 18` על מדף שקיבולתו 10.
   ב. Domain: `Shelf.LoadProduct` מחשב את המלאי החדש ומוודא שלא חורג מ‑`Capacity`. במקרה של חריגה הוא זורק `ShelfCapacityExceededException`.
   גץ. API: ממופה ל‑HTTP 400 עם קוד `SHELF_CAPACITY_EXCEEDED`.

### טסטים (Unit Tests)

בפרויקט `VendingMachine.Tests` קיימים טסטים בשכבת ה‑Domain בלבד (xUnit):

- בדיקת כללים בודדים:
  - `Cannot_Exceed_Max_Shelves`
  - `Cannot_Load_Wrong_Category`
  - `Cannot_Exceed_Shelf_Capacity`
  - `Successful_Purchase_Reduces_Inventory`
  - `Purchase_Fails_When_Out_Of_Stock`
- טסט End‑to‑End:
  - `EndToEnd_Scenarios_1_To_6` – מריץ ברצף את התרחישים מהמסמך כדי לוודא שהלוגיקה תואמת לדרישות.

הפקודה:

```bash
dotnet test
```

מריצה את כל הטסטים, וכולם עוברים (נכון למצב הנוכחי של הקוד).

### הנחות ושאלות

- **קטגוריות מוצרים (ProductCategory)**
  - הנחה: קטגוריות מיוצגות באמצעות `Guid` (`ProductCategoryId`) בלבד, בלי טבלה/ישות נפרדת שנשמרת בבסיס נתונים.

- **מוצרים ומחירים**
  - הנחה: מוצר מכיל רק `Id`, `Name`, `ProductCategoryId`. אין מחיר, מטבע, הנחות וכו', כי התרגיל מציין שאין צורך לטפל בסליקה.

- **התנהגות תחת עומס / Concurrency**
  - הנחה: מאחר ושכבת התשתית היא In‑Memory בלבד, לא בוצע טיפול מפורט ב‑Concurrency מעבר למה ש‑`ConcurrentDictionary` נותן.

- **שגיאות ולוקליזציה**
  - הנחה: קודי השגיאה (`PRODUCT_NOT_FOUND`, `MAX_SHELVES_REACHED` וכו') הם חלק מה‑Contract, והודעות הטקסט כרגע באנגלית בלבד.

- **אבטחה / Admins**
  - הנחה: האפליקציה לא כוללת Authentication/Authorization; כל הקריאות נתפסות כאילו מגיעות מ‑Admin מורשה.

- **שמירת זיכרון**
  - הנחה: In‑Memory מספק לצורך התרגיל. המעבר ל‑DB אמיתי יתבצע ע"י מימוש חדש של `IVendingMachineRepository` בשכבת Infrastructure, בלי לשנות את ה‑Domain או ה‑Application.

### התאמה לקריטריונים של התרגיל

- **מודל Domain** – ישויות עם התנהגות, כללים עסקיים ממומשים בתוך ה‑Entities וזורקים חריגות ייעודיות.
- **עיצוב API** – RESTful, קודי תשובה מתאימים, פורמט שגיאות אחיד דרך Middleware.
- **טסטים** – כיסוי של תרחישי הצלחה וכישלון מרכזיים, כולל תרחיש End‑to‑End.
- **איכות קוד** – שמות ברורים, הפרדת אחריות בין שכבות, תלות חד כיוונית (Domain ← Application ← Infrastructure ← Api).
- **הרחבה עתידית** – אפשרות להוסיף קטגוריות/מוצרים/כללי עסק נוספים ומעבר ל‑DB אמיתי ללא שינוי בשכבות העליונות.



