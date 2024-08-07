﻿namespace MobileBgWatch.ViewModels
{
    public class PagingViewModel
    {
        public int PageNumber { get; set; }

        public bool HasPreviousPage => PageNumber > 1;

        public int PreviousPageNumber => PageNumber - 1;

        public bool HasNextPage => PageNumber < PagesCount;

        public int NextPageNumber => PageNumber + 1;

        public int PagesCount => (int)Math.Ceiling((double)VehiclesCount / VehiclesPerPage);

        public int VehiclesCount { get; set; }

        public int VehiclesPerPage { get; set; }
    }
}
