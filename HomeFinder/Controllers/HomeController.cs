using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HomeFinder.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeFinder.Controllers
{
    public class HomeController : Controller
    {
        HomeContext db;
        List<Parsers.IParser> parsers;

        public HomeController(HomeContext context)
        {
            db = context;

            parsers = new List<Parsers.IParser>();
            parsers.Add(new Parsers.ParserDomofond());
        }

        

        [HttpGet]
        public IActionResult Index(int? id)
        {
            return View(db.Users.ToList());
        }

        
        
        /// <summary>
        /// Формирование представления с формой настроек фильтров
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Filtres(int? id)
        {
            User user = await db.Users.FirstOrDefaultAsync(u => u.UserId == id);
            ViewBag.UserId = id;
            if (user != null)
            {
                ViewBag.MinCost = user.MinCost;
                ViewBag.MaxCost = user.MaxCost;
                ViewBag.MinRoomCount = user.MinRoomCount;
                ViewBag.MaxRoomCount = user.MaxRoomCount;
            }

            return View();
        }

        
        
        /// <summary>
        /// Настройка фильтров пользователя
        /// </summary>
        [HttpPost]
        public IActionResult Filtres(int userId, int minCost, int maxCost, int minRoomCount, int maxRoomCount)
        {
            User user =  db.Users.FirstOrDefault(u => u.UserId == userId);

            if (user != null)
            {
                user.MinCost = minCost;
                user.MaxCost = maxCost;
                user.MinRoomCount = minRoomCount;
                user.MaxRoomCount = maxRoomCount;
                db.Update(user);
            }
            else
            {
                user = new User
                {
                    MinCost = minCost,
                    MaxCost = maxCost,
                    MinRoomCount = minRoomCount,
                    MaxRoomCount = maxRoomCount
                };
                db.Add(user);
            }

            db.SaveChanges();
            return Redirect($"~/Home/Index/{userId}");
        }


        
        [HttpGet]
        public IActionResult Addresses(int? id)
        {
            // Возвращение представления для добавления адреса
            var addressesOK = (from address in db.Addresses where address.UserId == id select address)
                .ToList()
                .Select((address, index) =>
                {
                    string value = address.Street + ", дом " + address.House;
                    if (address.Building != 0)
                        value += ", корпус " + address.Building;
                    return new Tuple<Address, string>(address, value);
                })
                .ToList();
            ViewBag.UserId = id;
            ViewBag.Addresses = addressesOK;


            return View();
        }
        
        
        
        /// <summary>
        /// Добавление нового адреса в список удовлетворяющих пользователя адресов
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="street">Улица</param>
        /// <param name="house">Номер здания</param>
        /// <param name="building">Номер корпуса</param>
        [HttpPost]
        public async Task<IActionResult> AddAddress(int userId, string street, string house, int? building)
        {
            Address address = new Address
            {
                UserId = userId,
                Street = street,
                House = house, 
                Building = building ?? 0
            };
            db.Add(address);
            await db.SaveChangesAsync();

            return Redirect($"~/Home/Addresses/{userId}");
        }
        
        
        
        /// <summary>
        /// Удаление адреса у пользователя userId
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteAddress(int userId, int addressId)
        {
            Address address = await db.Addresses.FirstOrDefaultAsync(a => a.AddressId == addressId);
            db.Addresses.Remove(address);
            await db.SaveChangesAsync();

            return Redirect($"~/Home/Addresses/{userId}");
        }


        
        /// <summary>
        /// Получение результатов
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GoodOffers(int? id)
        {
            if (id != null)
            {
                User user = await db.Users.FirstOrDefaultAsync(u => u.UserId == id);
                if (user == null)
                {
                    int p = 0;
                    // Пользователя с таким id нет.
                    throw new Exception("User with such id not exist");
                }

                if (user.MinCost > user.MaxCost)
                {
                    ViewBag.Message = "Минимальная цена не может превышать максимальную цену";
                    return Redirect($"~/Home/ErrorMessage/{id}");
                }
                if (user.MinRoomCount > user.MaxRoomCount)
                {
                    ViewBag.Message = "Минимальное число комнат не может превышать максимальное число комнат";
                    return Redirect($"~/Home/ErrorMessage/{id}");
                }

                string city = "moskva-c3584";
                string url = $"https://www.domofond.ru/arenda-kvartiry-{city}?PriceFrom={user.MinCost}&PriceTo={user.MaxCost}&RentalRate=Month&Rooms=";

                var rooms = new[]
                    {"Studio", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine"};
;               for (int i = user.MinRoomCount; i <= user.MaxRoomCount; i++)
                    url += rooms[i] + "%2C";

                url = url.Remove(url.Length - 3, 3);

                var offers = new List<Parsers.RentalOffer>();
                foreach (var parser in parsers)
                {
                    offers.AddRange(await parser.ParseByUrl(url));
                }

                var addressesOK = (from address in db.Addresses where address.UserId == id select address).ToList();
                var goodOffers = Filtrator.Filtration(offers, addressesOK);

                ViewBag.AllOffers = goodOffers;
                ViewBag.UserId = id;
                return View();
            }
            else
            {
                // Не указан id.
                throw new Exception("Id not specified");
            }
        }
    }
}
