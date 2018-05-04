using ComicBookShared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ComicBookLibraryManagerWebApp.ViewModels;
using System.Net;
using ComicBookShared.Data;
using System.Data.Entity;

namespace ComicBookLibraryManagerWebApp.Controllers
{
    /// <summary>
    /// Controller for the "Comic Books" section of the website.
    /// </summary>
    public class ComicBooksController : BaseController
    {
        public ActionResult Index()
        {
            //returns list of comic books
            var comicBooks = Context.ComicBooks
                .Include(cb => cb.Series)
                .OrderBy(cb => cb.Series.Title)
                .ThenBy(cb => cb.IssueNumber)
                .ToList();

            return View(comicBooks);
        }
     

        public ActionResult Detail(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //returns single comic book
            var comicBook = Context.ComicBooks
                    .Include(cb => cb.Series)
                    .Include(cb => cb.Artists.Select(a => a.Artist))
                    .Include(cb => cb.Artists.Select(a => a.Role))
                    .Where(cb => cb.Id == id)
                    .SingleOrDefault();

                if (comicBook == null)
            {
                return HttpNotFound();
            }

            // Sort the artists.
            comicBook.Artists = comicBook.Artists.OrderBy(a => a.Role.Name).ToList();

            return View(comicBook);
        }

        public ActionResult Add()
        {
            var viewModel = new ComicBooksAddViewModel();
            
            viewModel.Init(Context);

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Add(ComicBooksAddViewModel viewModel)
        {
            ValidateComicBook(viewModel.ComicBook);

            if (ModelState.IsValid)
            {
                var comicBook = viewModel.ComicBook;
                comicBook.AddArtist(viewModel.ArtistId, viewModel.RoleId);

                Context.ComicBooks.Add(comicBook);
                Context.SaveChanges();

                TempData["Message"] = "Your comic book was successfully added!";

                return RedirectToAction("Detail", new { id = comicBook.Id });
            }

            viewModel.Init(Context);

            return View(viewModel);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            var comicBook = Context.ComicBooks
                .Where(cb => cb.Id == id)
                .SingleOrDefault();

            if (comicBook == null)
            {
                return HttpNotFound();
            }

            var viewModel = new ComicBooksEditViewModel()
            {
                ComicBook = comicBook
            };
            viewModel.Init(Context);

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Edit(ComicBooksEditViewModel viewModel)
        {
            ValidateComicBook(viewModel.ComicBook);

            if (ModelState.IsValid)
            {
                var comicBook = viewModel.ComicBook;

                Context.Entry(comicBook).State = EntityState.Modified;
                Context.SaveChanges();

                TempData["Message"] = "Your comic book was successfully updated!";

                return RedirectToAction("Detail", new { id = comicBook.Id });
            }

            viewModel.Init(Context);

            return View(viewModel);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            var comicBook = Context.ComicBooks
                .Include(cb => cb.Series)
                .Where(cb => cb.Id == id)
                .SingleOrDefault();

            if (comicBook == null)
            {
                return HttpNotFound();
            }

            return View(comicBook);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            // TODO Delete the comic book.
            var comicBook = new ComicBook() {Id = id};
            Context.Entry(comicBook).State = EntityState.Deleted;
            Context.SaveChanges();

            TempData["Message"] = "Your comic book was successfully deleted!";

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Validates a comic book on the server
        /// before adding a new record or updating an existing record.
        /// </summary>
        /// <param name="comicBook">The comic book to validate.</param>
        private void ValidateComicBook(ComicBook comicBook)
        {
            if (ModelState.IsValidField("ComicBook.SeriesId") &&
                ModelState.IsValidField("ComicBook.IssueNumber"))
            {
                if (Context.ComicBooks
                    .Any(cb => cb.Id != comicBook.Id &&
                                cb.SeriesId == comicBook.SeriesId &&
                                cb.IssueNumber == comicBook.IssueNumber))
                {
                    ModelState.AddModelError("ComicBook.IssueNumber",
                        "The provided Issue Number has already been entered for the selected Series.");
                }
            }
        }
    }
}