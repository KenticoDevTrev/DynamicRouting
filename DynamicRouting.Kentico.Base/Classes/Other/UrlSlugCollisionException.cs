using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicRouting.Kentico.Classes
{
    /// <summary>
    /// Wrapper for normal Exceptions for when a Url Slug collission occurrs
    /// </summary>
    public class UrlSlugCollisionException : Exception
    {
        public UrlSlugCollisionException() : base()
        {

        }
        public UrlSlugCollisionException(string message) : base()
        {

        }

        public UrlSlugCollisionException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
