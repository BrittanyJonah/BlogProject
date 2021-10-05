using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalBlog.Services
{
    public interface ISlugService
    {
        string urlFriendly(string title);

        bool isUnique(string slug);
    }
}
