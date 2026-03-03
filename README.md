<div dir="rtl">

## מערכת VendingMachine – תרגיל בית

פתרון זה מממש מערכת Backend לניהול מכונות מכירה (Vending Machines) והמלאי שלהן, באמצעות .NET 8 Web API ובמבנה של **Clean Architecture**.

### מבנה הפתרון (Projects / Layers)

- **VendingMachine.Domain**  
  שכבת ה‑Domain – ישויות וחוקי העסק:
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
  שכבת האפליקציה (Use Cases):
  - DTOs לשימוש ה‑API: `VendingMachineDto`, `ShelfDto`, `ProductDto`
  - `IVendingMachineRepository` – ממשק לריפוזיטורי (בלי תלות בהטמעה)
  - `IVendingMachineService` + `VendingMachineService` – מימוש מקרי השימוש:
    - יצירת מכונת מכירה
    - הוספת מדף
    - טעינת מלאי
    - רכישת מוצר

- **VendingMachine.Infrastructure**  
  שכבת התשתית (Infrastructure):
  - `InMemoryVendingMachineRepository` – מימוש In‑Memory של `IVendingMachineRepository` עם `ConcurrentDictionary<Guid, VendingMachine>`.
  - אין בסיס נתונים אמיתי, אין EF Core, ואין תלות חיצונית.

- **VendingMachine.Api**  
  שכבת ה‑Web API (ASP.NET Core):
  - `VendingMachinesController` – Controller דק, שכל האחריות העסקית עוברת אליו דרך `IVendingMachineService`.
  - נקודות קצה (Endpoints):
    - `POST /api/machines` – יצירת מכונה
    - `GET  /api/machines/{machineId}` – שליפת מכונה
    - `POST /api/machines/{machineId}/shelves` – הוספת מדף
    - `POST /api/machines/{machineId}/shelves/{shelfId}/inventory` – טעינת מלאי למדף
    - `POST /api/machines/{machineId}/purchase` – רכישת מוצר
  - Middleware גלובלי לניהול שגיאות: `ExceptionHandlingMiddleware`, שמחזיר פורמט אחיד:
    ```json
    {
      "code": "ERROR_CODE",
      "message": "Readable message"
    }
    ```
  - Swagger / OpenAPI מופעל בדיבוג.

- **VendingMachine.Tests**  
  פרויקט טסטים (xUnit) עבור שכבת ה‑Domain בלבד.

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
   - API: `POST /api/machines`
     ```json
     {
       "name": "Main Lobby VM",
       "location": "Lobby",
       "maxShelves": 5
     }
     ```
   - Domain: קריאה ל‑`VendingMachineService.CreateVendingMachineAsync` שיוצר `VendingMachine` עם `MaxShelves = 5`.

2. **הוספת 3 מדפים: שתייה 10, חטיפים 20, שתייה 15**
   - נניח שני סוגי קטגוריות (מזוהים לפי Guid):
     - Drinks: `drinkCategoryId`
     - Snacks: `snackCategoryId`
   - API: `POST /api/machines/{machineId}/shelves` שלוש פעמים:
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
   - API: `POST /api/machines/{machineId}/shelves/{shelfId}/inventory`
     ```json
     {
       "productId": "<waterProductId>",
       "productName": "Water Bottle",
       "productCategoryId": "<drinkCategoryId>",
       "quantity": 5
     }
     ```
   - Domain: `VendingMachine.LoadInventory` → `Shelf.LoadProduct` מעדכן מלאי ומוודא קיבולת.

4. **ניסיון לטעון צ'יפס על מדף שתייה → כישלון**
   - אותו Endpoint כמו בסעיף 3, אבל עם `productCategoryId = <snackCategoryId>`.
   - Domain: `Shelf.ValidateProductCategory` זורק `InvalidProductCategoryException` אם הקטגוריה של המוצר לא תואמת את `ProductCategoryId` של המדף.
   - API: ה‑Middleware ממפה את החריגה ל‑HTTP 400 עם קוד `INVALID_PRODUCT_CATEGORY`.

5. **ניסיון להוסיף מדף שישי → כישלון**
   - אחרי הוספת 5 מדפים, עוד קריאה ל‑`POST /api/machines/{machineId}/shelves`:
   - Domain: `VendingMachine.AddShelf` בודק אם כמות המדפים הקיימת >= `MaxShelves` וזורק `MaxShelvesReachedException`.
   - API: ממופה ל‑HTTP 400 עם קוד `MAX_SHELVES_REACHED`.

6. **ניסיון לטעון 18 בקבוקי מים על מדף עם קיבולת 10 → כישלון**
   - `quantity: 18` על מדף שקיבולתו 10.
   - Domain: `Shelf.LoadProduct` מחשב את המלאי החדש ומוודא שלא חורג מ‑`Capacity`. במקרה של חריגה הוא זורק `ShelfCapacityExceededException`.
   - API: ממופה ל‑HTTP 400 עם קוד `SHELF_CAPACITY_EXCEEDED`.

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

### הנחות ושאלות (כפי שנתבקש בתרגיל)

- **קטגוריות מוצרים (ProductCategory)**
  - הנחה: קטגוריות מיוצגות באמצעות `Guid` (`ProductCategoryId`) בלבד, בלי טבלה/ישות נפרדת שנשמרת בבסיס נתונים.
  - שאלה: האם בגרסת Production נרצה API לניהול קטגוריות (CRUD מלא), או שקטגוריות מנוהלות במקום אחר במערכת המשחק?

- **מוצרים ומחירים**
  - הנחה: מוצר מכיל רק `Id`, `Name`, `ProductCategoryId`. אין מחיר, מטבע, הנחות וכו', כי התרגיל מציין שאין צורך לטפל בסליקה.
  - שאלה: איך תרצו לייצג מחירים, מטבעות שונים, והאם יש הפרדה בין מחיר "לשחקן" למחיר "פנימי" במשחק?

- **התנהגות תחת עומס / Concurrency**
  - הנחה: מאחר ושכבת התשתית היא In‑Memory בלבד, לא בוצע טיפול מפורט ב‑Concurrency מעבר למה ש‑`ConcurrentDictionary` נותן.
  - שאלה: במערכת אמיתית – האם נדרש מנגנון Concurrency (optimistic / pessimistic), במיוחד סביב רכישות במקביל מאותה מכונה?

- **שגיאות ולוקליזציה**
  - הנחה: קודי השגיאה (`PRODUCT_NOT_FOUND`, `MAX_SHELVES_REACHED` וכו') הם חלק מה‑Contract, והודעות הטקסט כרגע באנגלית בלבד.
  - שאלה: האם צריך לתמוך בשפות נוספות (למשל עברית/אנגלית לפי Header) והאם קוד השגיאה הוא חלק מ‑API ציבורי יציב?

- **אבטחה / Admins**
  - הנחה: האפליקציה לא כוללת Authentication/Authorization; כל הקריאות נתפסות כאילו מגיעות מ‑Admin מורשה.
  - שאלה: האם בעתיד תרצו להוסיף OAuth/JWT או אינטגרציה עם מערכת זהויות קיימת של המשחק?

- **Persistence אמיתי**
  - הנחה: In‑Memory מספק לצורך התרגיל. המעבר ל‑DB אמיתי יתבצע ע"י מימוש חדש של `IVendingMachineRepository` בשכבת Infrastructure, בלי לשנות את ה‑Domain או ה‑Application.

### התאמה לקריטריונים של התרגיל

- **מודל Domain** – ישויות עם התנהגות, כללים עסקיים ממומשים בתוך ה‑Entities וזורקים חריגות ייעודיות.
- **עיצוב API** – RESTful, קודי תשובה מתאימים, פורמט שגיאות אחיד דרך Middleware.
- **טסטים** – כיסוי של תרחישי הצלחה וכישלון מרכזיים, כולל תרחיש End‑to‑End.
- **איכות קוד** – שמות ברורים, הפרדת אחריות בין שכבות, תלות חד כיוונית (Domain ← Application ← Infrastructure ← Api).
- **הרחבה עתידית** – אפשרות להוסיף קטגוריות/מוצרים/כללי עסק נוספים ומעבר ל‑DB אמיתי ללא שינוי בשכבות העליונות.

</div>

