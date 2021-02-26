using FilmsCatalog.Data;
using FilmsCatalog.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using X.PagedList;

namespace FilmsCatalog.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private ApplicationDbContext db;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            db = context;
        }

        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page ?? 1; 
            int pageSize = 6; 
            var result = from film in db.Films
                         join u in db.Users on film.UserId equals u.Id
                         select new FilmViewModel() { Id = film.Id, Name = film.Name, Description = film.Description,
                         Director = film.Director, ReleaseYear = film.ReleaseYear, UserId = u.Id, UserName = u.FirstName};
            
            return View(await result.ToPagedListAsync(pageNumber, pageSize));
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(FilmViewModel filmVM)
        {
            Film film = new Film { Name = filmVM.Name, Description = filmVM.Description, Director = filmVM.Director, ReleaseYear = filmVM.ReleaseYear};
            film.UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (filmVM.Image != null)
                film.Image = GetImage(filmVM);
            db.Films.Add(film);
            await db.SaveChangesAsync();

            return Redirect("Index");
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if(id != null)
            {
                Film film = await db.Films.FirstOrDefaultAsync(x => x.Id == id);
                if (film != null)
                    return View(film);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(FilmViewModel filmVM)
        {
            if(filmVM.UserId == User.FindFirst(ClaimTypes.NameIdentifier).Value)
            {
                Film film = new Film { Id = filmVM.Id, Name = filmVM.Name, Description = filmVM.Description, Director = filmVM.Director, ReleaseYear = filmVM.ReleaseYear, UserId = filmVM.UserId };
                if (filmVM.Image != null)
                    film.Image = GetImage(filmVM);
                db.Films.Update(film);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id != null)
            {
                Film film = await db.Films.FirstOrDefaultAsync(p => p.Id == id);
                if (film != null)
                    return View(film);
            }
            return NotFound();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public byte[] GetImage(FilmViewModel filmVM)
        {
            byte[] imageData = null;
            using (var binaryReader = new BinaryReader(filmVM.Image.OpenReadStream()))
            {
                imageData = binaryReader.ReadBytes((int)filmVM.Image.Length);
            }
            return imageData;
        }

        public async Task<FileContentResult> GetImageView(int? id)
        {
            if (id != null)
            {
                Film film = await db.Films.FirstOrDefaultAsync(p => p.Id == id);
                if (film != null)
                {
                    return File(film.Image, "image/png");
                }
                else
                {
                    return null;
                }
            }
            return null;
        }
    }
}
