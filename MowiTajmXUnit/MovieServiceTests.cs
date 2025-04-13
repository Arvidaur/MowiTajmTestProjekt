using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using MowiTajm.Data;
using MowiTajm.Models;
using MowiTajm.Services;
using Xunit;

public class MovieServiceTests
{
    [Fact]
    public async Task GetMovieDetailsAsync_ReturnsCorrectData()
    {
        // Arrange: Skapa en unik, in-memory databas för att simulera databasen
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // unik databas för varje test
            .Options;

        using var context = new ApplicationDbContext(options);

        // Skapa en IMDb-ID och lägg till recensioner i databasen
        var imdbID = "tt1234567";
        context.Reviews.AddRange(new List<Review>
        {
            new Review { ImdbID = imdbID, Rating = 4 },
            new Review { ImdbID = imdbID, Rating = 2 }
        });

        await context.SaveChangesAsync(); // Spara ändringarna i databasen

        // Mocka OmdbService och sätt upp dess beteende
        var mockOmdbService = new Mock<IOmdbService>();
        mockOmdbService
            .Setup(service => service.GetMovieByIdAsync(imdbID)) // Simulera att OMDb-servicen returnerar en film med detta IMDb-ID
            .ReturnsAsync(new MovieFull { Title = "Test Movie", ImdbID = imdbID });

        // Skapa MovieService-instansen som vi vill testa
        var movieService = new MovieService(mockOmdbService.Object, context);

        // Act: Anropa metoden för att hämta filmens detaljer
        var (movie, reviews, averageRating) = await movieService.GetMovieDetailsAsync(imdbID);

        // Assert: Verifiera att resultatet är som förväntat
        Assert.Equal("Test Movie", movie.Title); // Kontrollera att filmtiteln är korrekt
        Assert.Equal(2, reviews.Count); // Kontrollera att vi har rätt antal recensioner
        Assert.Equal(3.0, averageRating); // Kontrollera att medelvärdet är korrekt (4 + 2) / 2
    }

    [Fact]
    public async Task GetMovieDetailsAsync_NoReviews_ReturnsZeroAverage()
    {
        // Arrange: Skapa en ny unik, in-memory databas för att simulera att inga recensioner finns
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var imdbID = "tt0000001"; // IMDb-ID för en film utan recensioner

        // Mocka OmdbService för att returnera en film utan recensioner
        var mockOmdbService = new Mock<IOmdbService>();
        mockOmdbService
            .Setup(service => service.GetMovieByIdAsync(imdbID))
            .ReturnsAsync(new MovieFull { Title = "Movie Without Reviews", ImdbID = imdbID });

        // Skapa MovieService-instansen
        var movieService = new MovieService(mockOmdbService.Object, context);

        // Act: Anropa metoden för att hämta filmens detaljer
        var (movie, reviews, averageRating) = await movieService.GetMovieDetailsAsync(imdbID);

        // Assert: Verifiera att inga recensioner och medelvärdet är 0
        Assert.Equal("Movie Without Reviews", movie.Title); // Kontrollera att filmtiteln är korrekt
        Assert.Empty(reviews); // Kontrollera att inga recensioner finns
        Assert.Equal(0, averageRating); // Kontrollera att medelvärdet är 0
    }

    [Fact]
    public async Task GetMovieDetailsAsync_ExceptionThrown_ReturnsDefaultValues()
    {
        // Arrange: Skapa en unik, in-memory databas för att simulera en exception
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var imdbID = "ttEXCEPTION"; // IMDb-ID som ska orsaka ett fel

        // Mocka OmdbService för att kasta ett undantag när denna film begärs
        var mockOmdbService = new Mock<IOmdbService>();
        mockOmdbService
            .Setup(service => service.GetMovieByIdAsync(imdbID))
            .ThrowsAsync(new Exception("Simulerat fel")); // Simulera ett fel vid anrop

        // Skapa MovieService-instansen
        var movieService = new MovieService(mockOmdbService.Object, context);

        // Act: Anropa metoden för att hämta filmens detaljer, vilket kommer att orsaka ett undantag
        var (movie, reviews, averageRating) = await movieService.GetMovieDetailsAsync(imdbID);

        // Assert: Verifiera att metoden returnerar standardvärden när ett undantag inträffar
        Assert.NotNull(movie); // Filmen ska inte vara null
        Assert.Equal(string.Empty, movie.Title); // Filmtiteln ska vara en tom sträng som standard
        Assert.Empty(reviews); // Inga recensioner ska returneras
        Assert.Equal(0, averageRating); // Medelvärdet ska vara 0
    }
}
