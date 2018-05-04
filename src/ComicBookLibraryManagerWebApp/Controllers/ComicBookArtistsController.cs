using ComicBookLibraryManagerWebApp.ViewModels;
using ComicBookShared.Models;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using ComicBookShared.Data;
using System.Linq;

namespace ComicBookLibraryManagerWebApp.Controllers
{
    /// <summary>
    /// Controller for adding/deleting comic book artists.
    /// </summary>
    public class ComicBookArtistsController : Controller
    {
        private Context _context = null;

        public ComicBookArtistsController()
        {
            _context = new Context();
        }

        public ActionResult Add(int comicBookId)
        {
            var comicBook = _context.ComicBooks
                    .Include(cb => cb.Series)
                    .Include(cb => cb.Artists.Select(a => a.Artist))
                    .Include(cb => cb.Artists.Select(a => a.Role))
                    .Where(cb => cb.Id == comicBookId)
                    .SingleOrDefault();

            if (comicBook == null)
            {
                return HttpNotFound();
            }

            var viewModel = new ComicBookArtistsAddViewModel()
            {
                ComicBook = comicBook
            };

            viewModel.Init(_context);

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Add(ComicBookArtistsAddViewModel viewModel)
        {
            ValidateComicBookArtist(viewModel);

            if (ModelState.IsValid)
            {
                var ComicBookArtist = new ComicBookArtist()
                {
                    ComicBookId = viewModel.ComicBookId,
                    ArtistId = viewModel.ArtistId,
                    RoleId = viewModel.RoleId
                };

                _context.ComicBookArtists.Add(ComicBookArtist);
                _context.SaveChanges();

                TempData["Message"] = "Your artist was successfully added!";

                return RedirectToAction("Detail", "ComicBooks", new { id = viewModel.ComicBookId });
            }

         
            viewModel.ComicBook = _context.ComicBooks
                    .Include(cb => cb.Series)
                    .Where(cb => cb.Id == viewModel.ComicBookId)
                    .SingleOrDefault();
            viewModel.Init(_context);

            return View(viewModel);
        }

        public ActionResult Delete(int comicBookId, int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // TODO Get the comic book artist.
            // Include the "ComicBook.Series", "Artist", and "Role" navigation properties.
            var comicBookArtist = _context.ComicBookArtists
                .Include(c => c.Artist)
                .Include(c => c.Role)
                .Include(c => c.ComicBook.Series)
                .Where(c => c.Id == (int)id)
                .SingleOrDefault();

            if (comicBookArtist == null)
            {
                return HttpNotFound();
            }

            return View(comicBookArtist);
        }

        [HttpPost]
        public ActionResult Delete(int comicBookId, int id)
        {
            // TODO Delete the comic book artist
            var comicBookArtist = new ComicBookArtist() { Id = id };
            _context.Entry(comicBookArtist).State = EntityState.Deleted;
            _context.SaveChanges();

            TempData["Message"] = "Your artist was successfully deleted!";

            return RedirectToAction("Detail", "ComicBooks", new { id = comicBookId });
        }

        /// <summary>
        /// Validates a comic book artist on the server
        /// before adding a new record.
        /// </summary>
        /// <param name="viewModel">The view model containing the values to validate.</param>
        private void ValidateComicBookArtist(ComicBookArtistsAddViewModel viewModel)
        {
            // If there aren't any "ArtistId" and "RoleId" field validation errors...
            if (ModelState.IsValidField("ArtistId") &&
                ModelState.IsValidField("RoleId"))
            {
                // Then make sure that this artist and role combination 
                // doesn't already exist for this comic book.
                // TODO Call method to check if this artist and role combination
                // already exists for this comic book.
                if (_context.ComicBookArtists
                    .Any(cba => cba.ComicBookId != viewModel.ComicBookId &&
                                cba.ArtistId == viewModel.ArtistId &&
                                cba.RoleId == viewModel.RoleId))
                                cba.RoleId == viewModel.RoleId))
                {
                    ModelState.AddModelError("ArtistId",
                        "This artist and role combination already exists for this comic book.");
                }
            }
        }

        private bool _disposed = false;

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _context.Dispose();
            }

            _disposed = true;

            base.Dispose(disposing);
        }
    }
}