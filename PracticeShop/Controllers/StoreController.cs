using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using PracticeShop.Models;
using PracticeShop.ViewModels;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace PracticeShop.Controllers
{
    [Authorize(Roles = "admin")]
    public class StoreController : Controller
    {
        private StoreContextDB db;

        public StoreController(StoreContextDB context)
        {
            db = context;
        }

        [HttpGet]
        public IActionResult ShowLibrary()
        {
            return View(Read());
        }

        [HttpGet]
        public IActionResult GamesList()
        {
            return View(db.Games.OrderBy(g => g.Name).ToList());
        }

        [HttpGet]
        public IActionResult AddGame()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddGame(Game model)
        {
            db.Games.Add(model);
            await db.SaveChangesAsync();
            return RedirectToAction("Store");
        }

        [HttpPost]
        public IActionResult AddToLibrary(int gameID)
        {
            return View("AddToLibrary", AddGameToLibrary(gameID));
        }

        [HttpPost]
        public IActionResult BuyGame(int gameID)
        {
            return View("BuyGame", AddGameToLibrary(gameID));
        }

        public String AddGameToLibrary(int gameID)
        {
            
            if (db.Libraries.Where(l => l.UserName == User.Identity.Name).ToList().Count < 1)
            {
                List<Game> games = new List<Game>();
                games.Add(db.Games.Find(gameID));
                string json = JsonSerializer.Serialize<List<Game>>(games);
                db.Libraries.Add(new Library(User.Identity.Name, json));
                db.SaveChanges();
                return "Игра была добавлена в Вашу библиотеку!";
            }
            else
            {
                if (!CheckGame(gameID))
                {
                    Write(gameID);
                    return "Игра была добавлена в Вашу библиотеку!";
                }
                else return "Игра уже находится в Вашей библиотеке!";
            }
        }

        public void Write(int gameID)
        {
            List<Library> library = db.Libraries.Where(l => l.UserName == User.Identity.Name).ToList();
            List<Game> games = JsonSerializer.Deserialize<List<Game>>(library.Last().GamesID);
            games.Add(db.Games.Find(gameID));
            string json = JsonSerializer.Serialize<List<Game>>(games);
            var user = db.Libraries.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            user.GamesID = json;
            db.SaveChanges();
        }

        public List<Game> Read()
        {
            List<Library> library = db.Libraries.Where(l => l.UserName == User.Identity.Name).ToList();
            List<Game> games = JsonSerializer.Deserialize<List<Game>>(library.Last().GamesID);
            return games;
        }

        public bool CheckGame(int gameID)
        {
            List<Game> games = Read();
            if (games.Where(g => g.ID == gameID).ToList().Count > 0)
            {
                return true;
            }
            else return false;
        }
    }
}
