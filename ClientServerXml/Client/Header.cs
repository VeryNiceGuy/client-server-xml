using System;

namespace Client
{
    class Header
    {
        public Guid Id { get; set; }
        public long DocumentLength { get; set; }
        public long ListLength { get; set; }
    }
}
