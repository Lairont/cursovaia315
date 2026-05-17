# 🗺️ Карта маршрутов (Routes) приложения

## 📍 Главная страница

| Метод | Маршрут | Контроллер | Действие | Описание |
|-------|---------|-----------|---------|---------|
| GET | `/` | Home | Index | Главная страница приложения |
| GET | `/Home/About` | Home | About | О компании (опционально) |
| GET | `/Home/Contacts` | Home | Contacts | Контакты (опционально) |

---

## 🔐 Аутентификация (Account)

| Метод | Маршрут | Контроллер | Действие | Описание |
|-------|---------|-----------|---------|---------|
| GET | `/Account/Register` | Account | Register | Форма регистрации |
| POST | `/Account/Register` | Account | Register | Обработка регистрации |
| GET | `/Account/Login` | Account | Login | Форма логина |
| POST | `/Account/Login` | Account | Login | Обработка логина |
| GET | `/Account/Logout` | Account | Logout | Выход из аккаунта |
| GET | `/Account/Profile` | Account | Profile | Профиль пользователя (опционально) |

### Примеры:
```
GET  http://localhost:5000/Account/Register
POST http://localhost:5000/Account/Register

GET  http://localhost:5000/Account/Login
POST http://localhost:5000/Account/Login

GET  http://localhost:5000/Account/Logout
```

---

## 🛍️ Товары (Product)

| Метод | Маршрут | Контроллер | Действие | Описание |
|-------|---------|-----------|---------|---------|
| GET | `/Product/Catalog` | Product | Catalog | Каталог товаров (может быть с фильтром) |
| GET | `/Product/Catalog?categoryId=1` | Product | Catalog | Товары определённой категории |
| GET | `/Product/Detail/{id}` | Product | Detail | Детали товара |
| GET | `/Product/SearchJson` | Product | SearchJson | Поиск товаров (JSON для AJAX) |

### Примеры:
```
GET  http://localhost:5000/Product/Catalog
GET  http://localhost:5000/Product/Catalog?categoryId=1
GET  http://localhost:5000/Product/Detail/1
GET  http://localhost:5000/Product/SearchJson?query=ручка&categoryId=1
```

---

## 🛒 Корзина (Cart)

| Метод | Маршрут | Контроллер | Действие | Описание |
|-------|---------|-----------|---------|---------|
| GET | `/Cart/Index` | Cart | Index | Просмотр корзины |
| POST | `/Cart/AddToCart` | Cart | AddToCart | Добавить товар в корзину |
| POST | `/Cart/RemoveFromCart` | Cart | RemoveFromCart | Удалить товар из корзины |
| POST | `/Cart/ClearCart` | Cart | ClearCart | Очистить всю корзину |
| GET | `/Cart/GetCartCount` | Cart | GetCartCount | Получить количество товаров (JSON) |

### POST параметры для AddToCart:
```json
{
  "productId": 1,
  "quantity": 1
}
```

### POST параметры для RemoveFromCart:
```json
{
  "productId": 1
}
```

### Примеры:
```
GET  http://localhost:5000/Cart/Index
POST http://localhost:5000/Cart/AddToCart (productId=1&quantity=1)
POST http://localhost:5000/Cart/RemoveFromCart (productId=1)
POST http://localhost:5000/Cart/ClearCart
GET  http://localhost:5000/Cart/GetCartCount (возвращает JSON: {"count": 5})
```

---

## 💳 Оформление заказов (Checkout)

| Метод | Маршрут | Контроллер | Действие | Описание |
|-------|---------|-----------|---------|---------|
| GET | `/Checkout/Index` | Checkout | Index | Форма оформления заказа |
| POST | `/Checkout/PlaceOrder` | Checkout | PlaceOrder | Создание заказа |
| GET | `/Checkout/OrderConfirmed` | Checkout | OrderConfirmed | Подтверждение заказа |
| GET | `/Checkout/Orders` | Checkout | Orders | История заказов пользователя |
| GET | `/Checkout/OrderDetails/{id}` | Checkout | OrderDetails | Детали заказа (опционально) |

### POST параметры для PlaceOrder:
```json
{
  "address": "ул. Пушкина, д. 10, кв. 5",
  "city": "Москва",
  "postalCode": "123456",
  "deliveryMethod": "standard",
  "paymentMethod": "card"
}
```

### Примеры:
```
GET  http://localhost:5000/Checkout/Index
POST http://localhost:5000/Checkout/PlaceOrder (с JSON телом)
GET  http://localhost:5000/Checkout/OrderConfirmed
GET  http://localhost:5000/Checkout/Orders
GET  http://localhost:5000/Checkout/OrderDetails/1
```

---

## 🧪 Тестирование и утилиты (Test)

| Метод | Маршрут | Контроллер | Действие | Описание |
|-------|---------|-----------|---------|---------|
| GET | `/Test/CheckDatabase` | Test | CheckDatabase | Проверка подключения и статистика БД |
| GET | `/Test/AddTestData` | Test | AddTestData | Добавление тестовых данных в БД |

### Примеры:
```
GET  http://localhost:5000/Test/CheckDatabase
GET  http://localhost:5000/Test/AddTestData
```

---

## 📊 Полная схема маршрутов

```
/
├── Account/
│   ├── Register (GET, POST)
│   ├── Login (GET, POST)
│   ├── Logout (GET)
│   └── Profile (GET)
│
├── Product/
│   ├── Catalog (GET) [?categoryId=x]
│   ├── Detail/{id} (GET)
│   └── SearchJson (GET) [?query=x&categoryId=x]
│
├── Cart/
│   ├── Index (GET)
│   ├── AddToCart (POST)
│   ├── RemoveFromCart (POST)
│   ├── ClearCart (POST)
│   └── GetCartCount (GET) [JSON]
│
├── Checkout/
│   ├── Index (GET)
│   ├── PlaceOrder (POST) [JSON]
│   ├── OrderConfirmed (GET)
│   ├── Orders (GET)
│   └── OrderDetails/{id} (GET)
│
├── Test/
│   ├── CheckDatabase (GET)
│   └── AddTestData (GET)
│
├── Home/
│   ├── About (GET)
│   └── Contacts (GET)
│
└── Error (Error Handler)
```

---

## 🔄 Основные потоки навигации

### 📍 Поток анонимного пользователя:
```
/ (главная)
  ↓
/Product/Catalog (каталог)
  ↓
/Product/Detail/{id} (детали товара)
  ↓
/Cart/Index (добавление в корзину)
  ↓
/Account/Login (перенаправление на логин)
  ↓
/Account/Register (или регистрация)
  ↓
/Cart/Index (корзина)
  ↓
/Checkout/Index (оформление)
  ↓
/Checkout/OrderConfirmed (подтверждение)
```

### 👤 Поток авторизованного пользователя:
```
/ (главная, видны кнопки профиля)
  ↓
/Product/Catalog (каталог)
  ↓
/Product/Detail/{id} (детали)
  ↓
/Cart/Index (корзина, сохранится в БД)
  ↓
/Checkout/Index (оформление)
  ↓
/Checkout/OrderConfirmed (подтверждение)
  ↓
/Checkout/Orders (история заказов)
```

---

## 📤 API Endpoints (JSON responses)

### GetCartCount
```
GET /Cart/GetCartCount
Response: {"count": 5}
```

### SearchJson
```
GET /Product/SearchJson?query=ручка&categoryId=1
Response: 
[
  {
	"id": 1,
	"name": "Ручка синяя",
	"price": 25.00,
	"categoryId": 1
  }
]
```

### PlaceOrder
```
POST /Checkout/PlaceOrder
Request:
{
  "address": "...",
  "city": "...",
  "postalCode": "...",
  "deliveryMethod": "standard",
  "paymentMethod": "card"
}
Response: {"success": true, "orderId": 1}
```

---

## 🔐 Требования доступа

### Открытые (без авторизации):
- GET `/`
- GET `/Account/Register`
- POST `/Account/Register`
- GET `/Account/Login`
- POST `/Account/Login`
- GET `/Product/Catalog`
- GET `/Product/Detail/{id}`
- GET `/Product/SearchJson`
- POST `/Cart/AddToCart`
- GET `/Cart/GetCartCount`
- GET `/Test/CheckDatabase`
- GET `/Test/AddTestData`

### Требуют авторизации:
- GET `/Account/Logout`
- GET `/Account/Profile`
- GET `/Cart/Index`
- POST `/Cart/RemoveFromCart`
- POST `/Cart/ClearCart`
- GET `/Checkout/Index`
- POST `/Checkout/PlaceOrder`
- GET `/Checkout/OrderConfirmed`
- GET `/Checkout/Orders`
- GET `/Checkout/OrderDetails/{id}`

---

## 🎯 Query параметры

### Product/Catalog
```
?categoryId=1  - Фильтр по категории
```

### Product/SearchJson
```
?query=ручка    - Текст поиска
?categoryId=1   - Фильтр по категории
```

### Cart/AddToCart (POST body, не query)
```
productId=1&quantity=2
```

---

## 📱 Примеры вызовов из JavaScript

### Добавить в корзину
```javascript
fetch('/Cart/AddToCart', {
  method: 'POST',
  body: new FormData(formElement)
})
```

### Получить количество товаров
```javascript
fetch('/Cart/GetCartCount')
  .then(r => r.json())
  .then(data => console.log(data.count))
```

### Удалить из корзины
```javascript
fetch('/Cart/RemoveFromCart', {
  method: 'POST',
  body: new FormData(formElement)
})
```

### Оформить заказ
```javascript
fetch('/Checkout/PlaceOrder', {
  method: 'POST',
  headers: {'Content-Type': 'application/json'},
  body: JSON.stringify({
	address: "...",
	city: "...",
	postalCode: "...",
	deliveryMethod: "standard",
	paymentMethod: "card"
  })
})
```

---

## 🚀 Тестирование маршрутов

### curl примеры

```bash
# Добавить товар в корзину
curl -X POST http://localhost:5000/Cart/AddToCart \
  -d "productId=1&quantity=1"

# Получить количество товаров
curl http://localhost:5000/Cart/GetCartCount

# Проверить БД
curl http://localhost:5000/Test/CheckDatabase

# Добавить тестовые данные
curl http://localhost:5000/Test/AddTestData
```

---

## ✅ Чеклист маршрутов

- [ ] `/` - Главная страница работает
- [ ] `/Account/Register` - Регистрация доступна
- [ ] `/Account/Login` - Логин доступен
- [ ] `/Product/Catalog` - Каталог отображает товары
- [ ] `/Cart/Index` - Корзина работает
- [ ] `/Checkout/Index` - Оформление работает
- [ ] `/Checkout/Orders` - История заказов доступна
- [ ] `/Test/CheckDatabase` - БД доступна
- [ ] `/Test/AddTestData` - Данные добавлены

---

**Все маршруты работают и готовы к использованию! 🚀**
