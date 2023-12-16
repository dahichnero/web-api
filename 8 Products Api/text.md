# Создание Web Api

Создадим простой Web Api. Основные возможности:
- аутентификация;
- вывод списка товаров;
- вывод списка категорий товаров;
- добавление, изменение и удаление товаров и категорий;
- создание заказа;
- вывод списка заказов;

## 1. Создание и настройка проекта

Создайте проект Web Api Asp.Net Core. Укажите "Использовать контроллеры" и "Поддержка OpenAPI".

Удалите "WeatherForecast.cs" и "WeatherForecastController.cs".

Установите пакеты Entity Framework Core для работы с Sqlite.

## 2. Создание контекста

Создайте папку Models.

Добавьте класс для продукта:
```cs
public class Product
{
    public int ProductId { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Photo { get; set; }

    public decimal Price { get; set; }

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;

}
```

Для категории:
```cs
public class Category
{
    public int CategoryId { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
}
```

Для заказа:
```cs
public class Order
{
    public int OrderId { get; set; }

    public DateTime Time { get; set; }

    public IdentityUser<int> User { get; set; } = null!;
    public int UserId { get; set; }
    
    [StringLength(250)]
    public string Address { get; set; } = string.Empty;
}
```

Используем в качестве пользователя `IdentityUser<int>`. Это позволит нам применить встроенные библиотеки Asp.Net для регистрации и входа в систему (Identity). Тип `int` - это тип первичного ключа. По умолчанию, если использовать просто `IdentityUser`, типом первичного ключа будет `string` и будет генерироваться `GUID`.

Для товаров в заказе:
```cs
[PrimaryKey(nameof(OrderId), nameof(ProductId))] // составной первичный ключ
public class OrderProduct
{
    public int OrderId { get; set; } 

    public int ProductId { get; set; }

    public int Count { get; set; }

    public decimal CurrentPrice { get; set; }

    public Order Order { get; set; } = null!;

    public Product Product { get; set; } = null!;
}
```

Чтобы применить Identity, мы должны создать контекст и унаследовать его от `IdentityDbContext`. Сперва установите пакет `Microsoft.AspNetCore.Identity.EntityFrameworkCore`. Далее:
- если мы хотим использовать просто пользователя `IdentityUser`, мы унаследуем от `IdentityDbContext<TUser>`;
- если мы хотим использовать пользователя `IdentityUser<T>` или тип, унаследованный от `IdentityUser`, мы унаследуем контекст от `IdentityDbContext<TUser, TRole, TKey>` (тип пользователя, тип роли, тип первичных ключей).

Создадим контекст:
```cs
public class ProductsContext :
    IdentityDbContext<IdentityUser<int>, IdentityRole<int>, int>
{

    public ProductsContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderProduct> OrderProducts { get; set; }
}
```

Добавим в `appsettings` строку подключения:
```js
"ConnectionStrings": {
	"sqlite": "Data Source = products.db"
}
```

Внедрим `DbContext` в контейнер:
```cs
string connectionString = builder.Configuration.GetConnectionString("sqlite") ?? 
    throw new ApplicationException("Не задана строка подключения");
builder.Services.AddDbContext<ProductsContext>(opt => opt.UseSqlite(connectionString));
```

Выполним миграцию:
```powershell
Add-Migration "initial"
```

И обновим БД:
```powershell
Update-Database
```

Откройте базу данных программой `DBeaver` (или другой программой, открывающей Sqlite). Постройте диаграмму. Изучите содержимое таблиц. Обратите внимение на таблицы, которые создает AspNet.

## 3. Добавление аутентификации

Установите пакет `Microsoft.AspNetCore.Authentication.JwtBearer`.

Добавьте в конвейер `app.UseAuthentication();`:
```cs
app.UseHttpsRedirection();
app.UseAuthentication(); // аутентификация
app.UseAuthorization();
app.MapControllers();
```

Создайте класс:
```cs
public static class KeyProvider
{
    private static string keyString = 
        "super_secret_string_123_poiuytrewqasdfghjklzxcvbnm"; // тут будет секретный ключ
        // хотя вообще, не стоит его оставлять в исходном коде при загрузке в репозитории

    public static byte[] Key => Encoding.Default.GetBytes(keyString);

}
```

Внедрите зависимость:
```cs
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // провереяем издателя
            ValidateAudience = false, // не проверяем аудиторию
            ValidateLifetime = true, // проверяем время жизни
            ValidateIssuerSigningKey = true, // проверяем ключ
            ValidIssuer = "ects",
            IssuerSigningKey = new SymmetricSecurityKey(KeyProvider.Key)
        };
    });
```

Создайте контроллер `ProductsController` (Пустой **контроллер API**, **не MVC**):
```cs
[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetProducts()
    {
        return Ok("Success");
    }
}
```

Запустите приложение и проверьте, что данный метод работает и отдает `200 OK` с сообщением `Success`.

Добавьте аттрибут `[Authorize]`:
```cs
[HttpGet]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public IActionResult GetProducts()
{
    return Ok("Success");
}
```

Запустите, и убедитесь, что кодом ответа станет `401`.

Создайте контроллер `UsersController` и добавьте в него метод `Login`:
```cs
[HttpGet]
public IActionResult Login()
{
    return Ok("login");
}
```

Проверьте работоспособность.

Добавьте еще один метод (см. презентации по JWT и соответствующее руководство):
```cs
private string getToken()
{
    // утверждения о пользователе
    List<Claim> claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, "1"), // id
        new Claim(ClaimTypes.Name, "Test"), // name
        new Claim(ClaimTypes.Role, "Client") // role
    };

    // настройки ключа
    SigningCredentials credentials = new SigningCredentials(
            new SymmetricSecurityKey(KeyProvider.Key), // секретный ключ
            SecurityAlgorithms.HmacSha256); // алгоритм

    // создаем токен
    var token = new JwtSecurityToken(
            issuer: "ects",
            notBefore: DateTime.Now,
            expires: DateTime.Now.AddHours(12),
            claims: claims,
            signingCredentials: credentials
        );
    // обрабатываем его, получая строку
    var handler = new JwtSecurityTokenHandler();
    string result = handler.WriteToken(token);
    // возвращаем результат
    return result;
}
```

Вызовите его в `Login`:
```cs
[HttpGet]
public IActionResult Login()
{
    return Ok(getToken());
}
```

Запустите и получите токен. Проверьте его с помощью https://jwt.io/

## 4. Регистрация и аутентификация

Добавим зависимость:
```cs
builder.Services
    .AddIdentity<IdentityUser<int>, IdentityRole<int>>(opt => { // добавляем Identity
        // настройки пароля
        opt.Password.RequiredLength = 8;
        opt.Password.RequireDigit = true;
        opt.Password.RequireLowercase = true;
        opt.Password.RequireUppercase = true;
        // настройки email
        opt.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ProductsContext>(); // добавляем для него поддержку EF
```


Внедрите в `UsersController` зависимости:
```cs
private UserManager<IdentityUser<int>> users;
private readonly SignInManager<IdentityUser<int>> signIn;

public UsersController(UserManager<IdentityUser<int>> users, 
    SignInManager<IdentityUser<int>> signIn)
{
    this.users = users;
    this.signIn = signIn;
}
```

Создайте папку `DataTransfer`. В ней создайте класс:
```cs
public class RegistrationDTO
{
    [Required]
    [MinLength(1)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
```

Добавьте метод регистрации:
```cs
[HttpPost("registration")]
public async Task<IActionResult> Registration(RegistrationDTO dto)
{
    var user = new IdentityUser<int>
    {
        UserName = dto.Username,
        Email = dto.Email
    };

    // создаем пользователя
    var result = await users.CreateAsync(user, dto.Password);
    // если неудача, то BadRequest
    if (!result.Succeeded)
    {
        return BadRequest(result.Errors);
    }
    // добаляем роль
    result = await users.AddToRoleAsync(user, "client");
    // если неудача, то BadRequest
    if (!result.Succeeded)
    {
        return BadRequest(result.Errors);
    }

    return Ok("Success");
}
```

Добавьте в `Program.cs` строки для начального посева ролей (после `app.Build()`):
```cs
using (var scope = app.Services.CreateScope())
{
    var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    if (!roles.Roles.Any())
    {
        await roles!.CreateAsync(new IdentityRole<int> { Name = "admin" });
        await roles!.CreateAsync(new IdentityRole<int> { Name = "client" });
    }
}
```

Запустите и проверьте регистрацию пользователя. Изучите содержимое таблиц базы данных с помощью DBeaver.

Создайте в `DataTransfer` класс:
```cs
public class LoginDTO
{
    public string Username { get; set; }
    public string Password { get; set; }

}
```

Измените метод `Login`:
```cs
[HttpPost("login")]
public async Task<IActionResult> Login(LoginDTO loginDto)
{
    var user = await users.FindByNameAsync(loginDto.Username);

    if (user is null)
    {
        return NotFound("Неверное имя пользователя или пароль");
    }

    if (!await users.CheckPasswordAsync(user, loginDto.Password))
    {
        return NotFound("Неверное имя пользователя или пароль");
    }

    var principal = await signIn.CreateUserPrincipalAsync(user);

    return Ok(getToken(principal));
}
```

И `getToken`:
```cs
private string getToken(ClaimsPrincipal principal)
{
   // утверждения о пользователе
   List<Claim> claims = principal.Claims.ToList();

   // настройки ключа
   SigningCredentials credentials = new SigningCredentials(
           new SymmetricSecurityKey(KeyProvider.Key), // секретный ключ
           SecurityAlgorithms.HmacSha256); // алгоритм

   // создаем токен
   var token = new JwtSecurityToken(
           issuer: "ects",
           notBefore: DateTime.Now,
           expires: DateTime.Now.AddHours(12),
           claims: claims,
           signingCredentials: credentials
       );
   // обрабатываем его, получая строку
   var handler = new JwtSecurityTokenHandler();
   string result = handler.WriteToken(token);
   // возвращаем результат
   return result;
}
```

Запустите, авторизуйтесь и проверьте токен с помощью jwt.io.

## 5. Поддержка аутентификации в Swagger

Вы можете проделать это *опционально*. Или отказаться от выполнения данного пункта и использовать для отправки запросов Postman.

Добавим поддержку аутентификации в Swagger для упрощения отладки и тестирования.
Для этого, согласно документации, следует просто изменить `builder.Services.AddSwaggerGen()` на:
```cs
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
})
```


## 6. Список категорий

Обратитесь к защищенному методу (`GetProducts`) с использованием токена (успех, 200) и без него (неудача, 401).

Зарегистрируйте еще одного пользователя и добавьте ему роль `admin` через базу данных (см. таблицу `AspNetUserRoles`).

Добавьте шаблонный элемент:
![](1.png)
![](2.png)

Добавьте для методов добавления, изменения и удаления аттрибут:
```cs
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "admin")]
```

Запустите и протестируйте методы:
- без токена (недоступно);
- с токеном клиента (недоступно, 403);
- с токеном администратора (доступно);

Добавьте "лишнюю" категорию. Измените ее. Удалите ее.

Добавьте категории "Бургеры", "Напитки", "Закуски", "Десерты".

## 7. Список продуктов

Добавьте в `ProductsController` `DbContext`:
```cs
private ProductsContext context;

public ProductsController(ProductsContext context)
{
    this.context = context;
}
```

Измените метод `GetProducts`:
```cs
[HttpGet]
public async Task<List<Product>> GetProducts()
{
    return await context.Products.Include(p => p.Category).ToListAsync();
}
```

Добавьте в `DataTransfer` класс `UpdateProductDTO`:
```cs
public class UpdateProductDTO
{
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; } = string.Empty;

    [Required]
    public decimal Price { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public Product ToProduct()
    {
        return new Product
        {
            Name = Name,
            Description = Description,
            Price = Price,
            CategoryId = CategoryId
        };
    }
}
```

Метод добавления:
```cs
[HttpPost("add")]
public async Task<IActionResult> AddProduct(UpdateProductDTO productDTO)
{
    if (!context.Categories.Any(c => c.CategoryId == productDTO.CategoryId))
    {
        return BadRequest("Invalid Category");
    }
    var product = productDTO.ToProduct();
    context.Products.Add(product);
    await context.SaveChangesAsync();
    return Ok(product);
}
```

Метод изменения:
```cs
[HttpPut("update/{id}")]
public async Task<IActionResult> UpdateProduct(UpdateProductDTO productDTO, int id)
{
    if (!context.Categories.Any(c => c.CategoryId == productDTO.CategoryId))
    {
        return BadRequest("Invalid Category");
    }
    var product = context.Products.Find(id);
    if (product is null)
    {
        return NotFound();
    }
    product.Name = productDTO.Name;
    product.Price = productDTO.Price;
    product.CategoryId = productDTO.CategoryId;
    product.Description = productDTO.Description;
    await context.SaveChangesAsync();
    return Ok(product);
}
```

Запустите и проверьте работоспособность. Добавьте `Authorize`, чтобы запретить действия не администраторам системы.
Самостоятельно реализуйте метод для вывода всех товаров указанной категории `/products/category/{id}`
Самостоятельно реализуйте метод удаления `/products/delete/{id}` и защитите его с помощью `Authorize`.

## 8. Загрузка изображения

Добавьте middleware:
```cs
app.UseStaticFiles();
```

Создайте каталог `wwwroot`. В нем создайте каталог `images`.

Добавьте в `ProductsController` метод:
```cs
[HttpPut("update/photo/{id}")]
public async Task<IActionResult> SetPhoto(int id, [FromForm]IFormFile file)
{

}
```

Скачайте пакет `SixLabors.ImageSharp` для работы с изображениями.

Реализуйте `SetPhoto`:
```cs
[HttpPut("update/photo/{id}")]
public async Task<IActionResult> SetPhoto(int id, [FromForm]IFormFile file)
{
    var product = context.Products.Find(id);
    if (product is null)
    {
        return NotFound();
    }

    if (file.Length > 2 * 1024 * 1024)
    {
        return BadRequest("Max image size is 2MB");
    }

    MemoryStream stream = new();
    try
    {
        await file.CopyToAsync(stream);
        var format = Image.DetectFormat(stream.ToArray());
        if (format.DefaultMimeType != "image/png")
        {
            return BadRequest("Invalid file format");
        }
    }
    catch (UnknownImageFormatException)
    {
        return BadRequest("Invalid file format");
    }
    finally
    {
        stream.Close();
    }

    return Ok();
}
```

Отправьте различные файлы через Postman, проверьте, что корректно проверяется размер и формат файла.
![](3.png)

Внедрите зависимость:
```cs
private ProductsContext context;
private readonly IWebHostEnvironment hosting;

public ProductsController(ProductsContext context, IWebHostEnvironment hosting)
{
    this.context = context;
    this.hosting = hosting;
}
```

Модифицируйте метод добавления изображения и проверьте его работоспособность.
```cs
[HttpPut("update/photo/{id}")]
public async Task<IActionResult> SetPhoto(int id, [FromForm]IFormFile file)
{
    // проверка на существование продукта
    var product = context.Products.Find(id);
    if (product is null)
    {
        return NotFound();
    }
    // проверка размера файла
    if (file.Length > 2 * 1024 * 1024)
    {
        return BadRequest("Max image size is 2MB");
    }
    // открываем поток для чтения
    var stream = file.OpenReadStream();
    try
    {
        // проверяем формат файла
        var format = await Image.DetectFormatAsync(stream);
        if (format.DefaultMimeType != "image/png")
        {
            return BadRequest("Invalid file format");
        }
    }
    catch (UnknownImageFormatException)
    {
        return BadRequest("Invalid file format");
    }

    // если все правильно, то сохраняем в файл
    string filename = Path.Combine(hosting.WebRootPath, "images", Path.GetRandomFileName() + ".png");
    using (FileStream fs = new FileStream(filename, FileMode.Create))
    {
        stream.Position = 0;
        stream.CopyTo(fs);
    }
    // сохраним имя файла в БД
    product.Photo = Path.GetFileName(filename);
    context.SaveChanges();
    return Ok();

}
```

Защитите метод с помощью `Authorize`.

Добавьте с помощью api несколько товаров и изображений.


## 9. Оформление заказа

Создайте контроллер API `OrdersController`:
```cs
private ProductsContext context;
private readonly UserManager<IdentityUser<int>> userManager;

public OrdersController(ProductsContext context, UserManager<IdentityUser<int>> userManager)
{
    this.context = context;
    this.userManager = userManager;
}
```

Добавьте в `DataTransfer` класс:
```cs
public class CreateOrderDTO
{
    public class ProductsCount
    {
        public int ProductId { get; set; }

        [Range(1, 30)]
        public int Count { get; set; }
    }

    [Required]
    public string Address { get; set; }

    [Required]
    public List<ProductsCount> Products { get; set; } = new();

}
```

В контроллер `OrdersController` метод:
```cs
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[HttpPost("create")]
public async Task<IActionResult> CreateOrder(CreateOrderDTO createDto)
{
    return Ok();
}
```

В методе мы сперва получим Id пользователя из Claims, используя свойство `User`:
```cs
var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
```

Проверим, что каждый продукт в заказе встречается один раз:
```cs
if (createDto.Products.DistinctBy(p => p.ProductId).Count() != createDto.Products.Count)
{
    return BadRequest("Products in order must be unique");
}
```

Получим по id продуктов их список:
```cs
var products = createDto.Products.Select(p => context.Products.Find(p.ProductId));
```

Изменим предыдущую строку, получив пару значений: количество и продукт.
```cs
var products = createDto.Products.Select(p => new
{
    Product = context.Products.Find(p.ProductId),
    Count = p.Count
});
```

Проверим, что по всем Id удалось найти продукт:
```cs
if (products.Any(p => p.Product is null))
{
    return BadRequest("Invalid product id");
}
```

Создадим и добавим заказ:
```cs
var order = new Order
{
    Address = createDto.Address,
    Time = DateTime.Now,
    UserId = userId
};
context.Orders.Add(order);
```

Добавим записи в `OrderProduct`:
```cs
context.OrderProducts.AddRange(products.Select(p => new OrderProduct
{
    Count = p.Count,
    CurrentPrice = p.Product!.Price,
    ProductId = p.Product!.ProductId,
    Order = order
}));
```

Вызовем `SaveChanges` для отправки Sql-запросов:
```cs
await context.SaveChangesAsync();
```

Проверьте работоспособность. Запустите программу и попробуйте создать заказ.

## 10. История заказов

Самостоятельно реализуйте вывод истории заказов.
Каждый JSON-объект (заказ) должен включать в себя следующие поля:
- id заказа;
- id пользователя;
- адрес;
- время заказа;
- список товаров и их количеств.

Про каждый товар необходимо вывести:
- id товара;
- название товара;
- цена, по которой он был куплен;
- изображение;
- количество;

Создайте для решения задачи два новых типа `OrderInfoDTO` и `ProductInOrderDTO`.

Примерный результат:
```cs
[
  {
    "userId": 4,
    "orderId": 0,
    "time": "2023-11-29T09:11:17.725537",
    "address": "Первомайская 73",
    "products": [
      {
        "productId": 1,
        "name": "test123",
        "photo": "photo.png",
        "price": 123,
        "count": 20
      },
      {
        "productId": 2,
        "name": "test",
        "photo": "",
        "price": 500,
        "count": 10
      }
    ]
  },
  {
    "userId": 4,
    "orderId": 0,
    "time": "2023-11-29T09:11:22.5335924",
    "address": "Первомайская 73",
    "products": [
      {
        "productId": 1,
        "name": "test123",
        "photo": "photo.png",
        "price": 123,
        "count": 20
      }
    ]
  },
  {
    "userId": 4,
    "orderId": 0,
    "time": "2023-11-29T09:14:00.4248033",
    "address": "Первомайская 73",
    "products": [
      {
        "productId": 1,
        "name": "test123",
        "photo": "photo.png",
        "price": 123,
        "count": 10
      },
      {
        "productId": 2,
        "name": "test",
        "photo": "",
        "price": 500,
        "count": 20
      }
    ]
  }
]
```

Примерный код получения списка `OrderInfoDTO`:
```cs
var orders = context
    .Orders
    .Where(o => o.UserId == userId)
    .Select(o => new OrderInfoDTO
    {
        Address = o.Address,
        Time = o.Time,
        UserId = userId,
        Products = context.OrderProducts
            .Include(o => o.Product)
            .Where(op => op.OrderId == o.OrderId)
            .Select(op => new ProductInOrderDTO
            {
                Count = op.Count,
                Name = op.Product.Name,
                Photo = op.Product.Photo,
                Price = op.CurrentPrice,
                ProductId = op.Product.ProductId
            })
            .ToList()
    }); 
```

## 11. Разворачивание приложения

Опубликуйте приложение в Docker-контейнер.