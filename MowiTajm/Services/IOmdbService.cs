using System.Threading.Tasks;
using MowiTajm.Models;

namespace MowiTajm.Services
{
    public interface IOmdbService
    {
        Task<MovieFull> GetMovieByIdAsync(string imdbID);
    }
}