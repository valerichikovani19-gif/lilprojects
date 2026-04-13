// Copyright (C) TBC Bank. All Rights Reserved.
using Discounts.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace Discounts.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOfferService _offerService;
        private readonly ICategoryService _categoryService;

        public HomeController(IOfferService offerService, ICategoryService categoryService)
        {
            _offerService = offerService;
            _categoryService = categoryService;
        }

        // GET: /Home/Index
        public async Task<IActionResult> Index(
            string? search,
            int? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            int pageIndex = 1,
            CancellationToken cancellationToken = default)
        {
            var pagedOffers = await _offerService.GetActiveOffersAsync(
                search, categoryId, minPrice, maxPrice, pageIndex, 9, cancellationToken).ConfigureAwait(false);
            var categories = await _categoryService.GetAllCategoriesAsync(cancellationToken).ConfigureAwait(false);

            ViewBag.Categories = new SelectList(categories, "Id", "Name", categoryId);
            ViewBag.CurrentPage = pagedOffers.PageNumber;
            ViewBag.TotalPages = pagedOffers.TotalPages;

            ViewBag.HasPrevious = pagedOffers.HasPreviousPage;
            ViewBag.HasNext = pagedOffers.HasNextPage;
            //passing filters
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategory = categoryId;
            ViewBag.CurrentMinPrice = minPrice;
            ViewBag.CurrentMaxPrice = maxPrice;

            return View(pagedOffers.Items);
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        {
            if (id <= 0) return BadRequest();
            //fecthing by id
            var offer = await _offerService.GetOfferByIdAsync(id, cancellationToken).ConfigureAwait(false);

            if (offer == null) return NotFound();

            return View(offer);
        }

        // GET:  /Home/Error
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new Models.ViewModels.ErrorViewModel
            {
                RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
