﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using fa18Team22.Models;
using fa18Team22.DAL;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using fa18Team22.Utilities;
using Microsoft.AspNetCore.Authorization;
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace fa18Team22.Controllers
{

    public class ProcurementSearchController : Controller
    {

        private AppDbContext _db;
        public ProcurementSearchController(AppDbContext context)
        {
            _db = context;
        }

        // GET: /<Search Controller>/
        [Authorize(Roles = "Manager")]
        public IActionResult ManualProcurement()
        {

            ViewBag.TotalBooks = _db.Books.Count();

            var query = from r in _db.Books select r;
            List<Book> allBooks = new List<Book>();
            query = query.Include(b => b.Reviews);
            allBooks = query.ToList();

            List<Procurement> allprocs = new List<Procurement>();
            var procquery = from p in _db.Procurements select p;
            procquery = procquery.Include(p => p.Book).Include(p => p.Employee);
            allprocs = procquery.ToList();

            String strUserId = User.Identity.Name;
            AppUser apvmuser = _db.Users.FirstOrDefault(u => u.UserName == strUserId);

            List<AddProcurementVM> BooksToOrder = new List<AddProcurementVM>();
            foreach (Book book in allBooks)
            {
                AddProcurementVM apvm = new AddProcurementVM();
                apvm.Title = book.Title;
                apvm.ProcurementDate = System.DateTime.Today;
                apvm.BookID = book.BookID;
                apvm.Author = book.Author;
                apvm.AvgRatingProc = (decimal)book.AvgRating;
                apvm.PublishDate = book.PublishDate;
                apvm.Cost = book.BookCost;
                apvm.userID = User.Identity.Name;
                apvm.Inventory = book.Inventory;
                apvm.InventoryMinimum = book.ReplenishMinimum;
                apvm.SellingPrice = book.SalesPrice;
                apvm.ProfitMargin = ((Decimal)book.AvgSalesPrice - (Decimal)book.AvgBookCost);
                apvm.IncludeInProcurement = false;
                apvm.QuantityToOrder = 5;
                BooksToOrder.Add(apvm);

                foreach (Procurement proc in allprocs)
                {
                    if (proc.ProcurementStatus == false)
                    {
                        if (book.BookID == proc.Book.BookID)
                        {
                            BooksToOrder.Remove(apvm);
                        }
                    }
                }

            }
            ViewBag.SelectedBooksCount = BooksToOrder.Count();
            return View(BooksToOrder);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public IActionResult ManualProcurement(List<AddProcurementVM> procurementVMs)
        {

            foreach (AddProcurementVM apvm in procurementVMs)
            {

                //check to see if cost and quantity is greater than zero
                if (apvm.Cost <= 0 || apvm.QuantityToOrder <= 0)
                {
                    var query = from r in _db.Books select r;
                    //change to < r.ReplenishMinimum, exclude books on active procurement
                    List<Book> allBooks = new List<Book>();
                    query = query.Where(r => r.Inventory <= r.ReplenishMinimum);
                    query = query.Include(r => r.Procurements).Include(r => r.Reviews);
                    allBooks = query.ToList();

                    List<Procurement> allprocs = new List<Procurement>();
                    var procquery = from p in _db.Procurements select p;
                    procquery = procquery.Include(p => p.Book).Include(p => p.Employee);
                    allprocs = procquery.ToList();

                    String strUserId = User.Identity.Name;
                    AppUser apvmuser = _db.Users.FirstOrDefault(u => u.UserName == strUserId);


                    List<AddProcurementVM> BooksToOrder = new List<AddProcurementVM>();
                    foreach (Book book in allBooks)
                    {
                        AddProcurementVM apvm2 = new AddProcurementVM();
                        apvm2.Title = book.Title;
                        apvm2.ProcurementDate = System.DateTime.Today;
                        apvm2.BookID = book.BookID;
                        apvm2.Author = book.Author;
                        apvm2.AvgRatingProc = (decimal)book.AvgRating;
                        apvm2.Cost = book.BookCost;
                        apvm2.userID = User.Identity.Name;
                        apvm2.Inventory = book.Inventory;
                        apvm2.InventoryMinimum = book.ReplenishMinimum;
                        apvm2.SellingPrice = book.SalesPrice;
                        apvm2.ProfitMargin = ((Decimal)book.AvgSalesPrice - (Decimal)book.AvgBookCost);
                        apvm2.IncludeInProcurement = false;
                        apvm2.QuantityToOrder = 5;
                        BooksToOrder.Add(apvm2);

                        foreach (Procurement proc in allprocs)
                        {
                            if (proc.ProcurementStatus == false)
                            {
                                if (book.BookID == proc.Book.BookID)
                                {
                                    BooksToOrder.Remove(apvm2);
                                }
                            }
                        }

                    }
                    ViewBag.ProcurementError = "Quantity and Cost needs to be greater than zero";
                    return View("DetailedMProcurement", BooksToOrder);
                }
                else
                {
                    ViewBag.ProcurementError = "";
                }







                if (apvm.IncludeInProcurement == true)
                {
                    Book apvmbook = _db.Books.FirstOrDefault(r => r.BookID == apvm.BookID);
                    string strID = apvm.userID;
                    AppUser apvmuser = _db.Users.FirstOrDefault(u => u.UserName == apvm.userID);


                    Procurement procurement = new Procurement() { Book = apvmbook, Employee = apvmuser };
                    procurement.Price = apvm.Cost;
                    procurement.ProcurementDate = apvm.ProcurementDate;
                    procurement.ProcurementStatus = false;
                    procurement.Quantity = apvm.QuantityToOrder;

                    String userId = User.Identity.Name;
                    AppUser user = _db.Users.FirstOrDefault(u => u.UserName == userId);
                    procurement.Employee = user;

                    //update cost to be latest cost paid
                    apvmbook.BookCost = apvm.Cost;

                    _db.Books.Update(apvmbook);
                    _db.Procurements.Add(procurement);
                    _db.SaveChanges();
                }
            }

            return RedirectToAction("Index","Procurement");
        }

        [Authorize(Roles = "Manager")]
        public ActionResult DetailedSearch()
        {
            ViewBag.AllGenres = GetAllGenres();

            return View();
        }

        [Authorize(Roles = "Manager")]
        public IActionResult DetailedMProcurement(string SearchTitle, string SearchAuthor, string SearchUniqueID, int SearchGenre, DisplayBooks SelectedStock, SortOrderOpt SortButton)
        {
            List<Book> SelectedBooks = new List<Book>();

            var query = from r in _db.Books select r;

            //title
            if (!string.IsNullOrEmpty(SearchTitle))
            {
                query = query.Where(r => r.Title.Contains(SearchTitle));
            }

            //author
            if (!string.IsNullOrEmpty(SearchAuthor))
            {
                query = query.Where(r => r.Author.Contains(SearchAuthor));
            }

            //unique number
            if (!string.IsNullOrEmpty(SearchUniqueID))
            {
                int intUniqueNumber;
                try
                {
                    intUniqueNumber = Convert.ToInt32(SearchUniqueID);
                }
                catch
                {
                    //Add a message for the viewbag
                    ViewBag.Message = "You must enter a valid unique ID";

                    //re-populate drop down
                    //ViewBag.AllGenres .....

                    //Send user back to home page
                    return View("DetailedSearch"); //may need to change what goes in ""
                }

                query = query.Where(r => r.UniqueID == intUniqueNumber);

            }



            //genre
            if (SearchGenre != 0) // 0 = they chose "all genres" from the drop-down
            {
                Genre GenreToDisplay = _db.Genres.Find(SearchGenre);
                query = query.Where(r => r.Genre == GenreToDisplay);
            }


            //selected books - all or in stock only
            switch (SelectedStock)
            {
                case DisplayBooks.AllBooks:
                    break;
                case DisplayBooks.InStock:
                    query = query.Where(r => r.Inventory > 0);
                    break;
                default:
                    break;
            }

            SelectedBooks = query.ToList();
            ViewBag.SelectedBooksCount = SelectedBooks.Count();
            ViewBag.TotalBooks = _db.Books.Count();

            switch (SortButton)
            {
                case SortOrderOpt.DontSort: break;
                case SortOrderOpt.Title:
                    return View("Index", SelectedBooks.OrderBy(r => r.Title));
                case SortOrderOpt.Author:
                    return View("Index", SelectedBooks.OrderBy(r => r.Author));
                case SortOrderOpt.MostPopular:
                    return View("Index", SelectedBooks.OrderBy(r => r.BookID));
                case SortOrderOpt.Newest:
                    return View("Index", SelectedBooks.OrderByDescending(r => r.PublishDate));
                case SortOrderOpt.Oldest:
                    return View("Index", SelectedBooks.OrderBy(r => r.PublishDate));
                case SortOrderOpt.HighestRating:
                    return View("Index", SelectedBooks.OrderByDescending(r => r.AvgRating));
            }


            SelectedBooks = query.Include(r => r.Reviews).Include(r => r.Genre).ToList();
            ViewBag.SelectedBooksCount = SelectedBooks.Count();
            ViewBag.TotalBooks = _db.Books.Count();


            List<Procurement> allprocs = new List<Procurement>();
            var procquery = from p in _db.Procurements select p;
            procquery = procquery.Include(p => p.Book).Include(p => p.Employee);
            allprocs = procquery.ToList();

            String strUserId = User.Identity.Name;
            AppUser apvmuser = _db.Users.FirstOrDefault(u => u.UserName == strUserId);

            List<AddProcurementVM> BooksToOrder = new List<AddProcurementVM>();
            foreach (Book book in SelectedBooks)
            {
                AddProcurementVM apvm = new AddProcurementVM();
                apvm.Title = book.Title;
                apvm.ProcurementDate = System.DateTime.Today;
                apvm.BookID = book.BookID;
                apvm.Author = book.Author;
                apvm.AvgRatingProc = (decimal)book.AvgRating;
                apvm.PublishDate = book.PublishDate;
                apvm.Cost = book.BookCost;
                apvm.userID = User.Identity.Name;
                apvm.Inventory = book.Inventory;
                apvm.InventoryMinimum = book.ReplenishMinimum;
                apvm.SellingPrice = book.SalesPrice;
                apvm.ProfitMargin = ((Decimal)book.AvgSalesPrice - (Decimal)book.AvgBookCost);
                apvm.IncludeInProcurement = false;
                apvm.QuantityToOrder = 5;
                BooksToOrder.Add(apvm);

                foreach (Procurement proc in allprocs)
                {
                    if (proc.ProcurementStatus == false)
                    {
                        if (book.BookID == proc.Book.BookID)
                        {
                            BooksToOrder.Remove(apvm);
                        }
                    }
                }

            }
            if(BooksToOrder.Count == 0)
            {
                ViewBag.SelectedBooksCount = 0;
            }
            ViewBag.DetailedMError = "";
            return View(BooksToOrder);
        }
       



        [HttpPost]
        [Authorize(Roles = "Manager")]
        public IActionResult DetailedMProcurement(List<AddProcurementVM> procurementVMs)
        {

            foreach (AddProcurementVM apvm in procurementVMs)
            {
                if (apvm.IncludeInProcurement == true)
                {
                    if(apvm.Cost <= 0 || apvm.QuantityToOrder <= 0)
                    {
                        List<Int32> booksinquery = new List<Int32>();
                        foreach(AddProcurementVM returnbook in procurementVMs)
                        {
                            //Book booktoadd = _db.Books.FirstOrDefault(r => r.BookID == returnbook.BookID);
                            booksinquery.Add(returnbook.BookID);
                        }

                        ViewBag.DetailedMError = "Order and Cost need to be greater than zero";
                        ViewBag.AllGenres = GetAllGenres();
                        return View("DetailedSearch");
                    }
                    else
                    {
                        Book apvmbook = _db.Books.FirstOrDefault(r => r.BookID == apvm.BookID);
                        string strID = apvm.userID;
                        AppUser apvmuser = _db.Users.FirstOrDefault(u => u.UserName == apvm.userID);


                        Procurement procurement = new Procurement() { Book = apvmbook, Employee = apvmuser };
                        procurement.Price = apvm.Cost;
                        procurement.ProcurementDate = apvm.ProcurementDate;
                        procurement.ProcurementStatus = false;
                        procurement.Quantity = apvm.QuantityToOrder;

                        String userId = User.Identity.Name;
                        AppUser user = _db.Users.FirstOrDefault(u => u.UserName == userId);
                        procurement.Employee = user;

                        _db.Procurements.Add(procurement);
                        _db.SaveChanges();
                    }

                }
            }
            ViewBag.DetailedMError = "";
            return RedirectToAction("Index", "Procurement");
        }





        // GET: Books/Details/5
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _db.Books.Include(m => m.Genre).Include(r => r.Reviews)
                .FirstOrDefaultAsync(m => m.BookID == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }







        public SelectList GetAllGenres()
        {
            List<Genre> Genres = _db.Genres.ToList();

            Genre SelectNone = new Genre() { GenreID = 0, GenreName = "All Genres" };
            Genres.Add(SelectNone);


            //convert list to select list
            SelectList AllGenres = new SelectList(Genres.OrderBy(g => g.GenreID), "GenreID", "GenreName");

            //return the select list
            return AllGenres;
        }



    }
}