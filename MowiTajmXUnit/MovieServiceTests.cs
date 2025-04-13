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
        // Arrange: Skapa en unik, in-memory databas f�r att simulera databasen
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // unik databas f�r varje test
            .Options;

        using var context = new ApplicationDbContext(options);

        // Skapa en IMDb-ID och l�gg till recensioner i databasen
        var imdbID = "tt1234567";
        context.Reviews.AddRange(new List<Review>
        {
            new Review { ImdbID = imdbID, Rating = 4 },
            new Review { ImdbID = imdbID, Rating = 2 }
        });

        await context.SaveChangesAsync(); // Spara �ndringarna i databasen

        // Mocka OmdbService och s�tt upp dess beteende
        var mockOmdbService = new Mock<IOmdbService>();
        mockOmdbService
            .Setup(service => service.GetMovieByIdAsync(imdbID)) // Simulera att OMDb-servicen returnerar en film med detta IMDb-ID
            .ReturnsAsync(new MovieFull { Title = "Test Movie", ImdbID = imdbID });

        // Skapa MovieService-instansen som vi vill testa
        var movieService = new MovieService(mockOmdbService.Object, context);

        // Act: Anropa metoden f�r att h�mta filmens detaljer
        var (movie, reviews, averageRating) = await movieService.GetMovieDetailsAsync(imdbID);

        // Assert: Verifiera att resultatet �r som f�rv�ntat
        Assert.Equal("Test Movie", movie.Title); // Kontrollera att filmtiteln �r korrekt
        Assert.Equal(2, reviews.Count); // Kontrollera att vi har r�tt antal recensioner
        Assert.Equal(3.0, averageRating); // Kontrollera att medelv�rdet �r korrekt (4 + 2) / 2
    }

    [Fact]
    public async Task GetMovieDetailsAsync_NoReviews_ReturnsZeroAverage()
    {
        // Arrange: Skapa en ny unik, in-memory databas f�r att simulera att inga recensioner finns
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var imdbID = "tt0000001"; // IMDb-ID f�r en film utan recensioner

        // Mocka OmdbService f�r att returnera en film utan recensioner
        var mockOmdbService = new Mock<IOmdbService>();
        mockOmdbService
            .Setup(service => service.GetMovieByIdAsync(imdbID))
            .ReturnsAsync(new MovieFull { Title = "Movie Without Reviews", ImdbID = imdbID });

        // Skapa MovieService-instansen
        var movieService = new MovieService(mockOmdbService.Object, context);

        // Act: Anropa metoden f�r att h�mta filmens detaljer
        var (movie, reviews, averageRating) = await movieService.GetMovieDetailsAsync(imdbID);

        // Assert: Verifiera att inga recensioner och medelv�rdet �r 0
        Assert.Equal("Movie Without Reviews", movie.Title); // Kontrollera att filmtiteln �r korrekt
        Assert.Empty(reviews); // Kontrollera att inga recensioner finns
        Assert.Equal(0, averageRating); // Kontrollera att medelv�rdet �r 0
    }

    [Fact]
    public async Task GetMovieDetailsAsync_ExceptionThrown_ReturnsDefaultValues()
    {
        // Arrange: Skapa en unik, in-memory databas f�r att simulera en exception
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var imdbID = "ttEXCEPTION"; // IMDb-ID som ska orsaka ett fel

        // Mocka OmdbService f�r att kasta ett undantag n�r denna film beg�rs
        var mockOmdbService = new Mock<IOmdbService>();
        mockOmdbService
            .Setup(service => service.GetMovieByIdAsync(imdbID))
            .ThrowsAsync(new Exception("Simulerat fel")); // Simulera ett fel vid anrop

        // Skapa MovieService-instansen
        var movieService = new MovieService(mockOmdbService.Object, context);

        // Act: Anropa metoden f�r att h�mta filmens detaljer, vilket kommer att orsaka ett undantag
        var (movie, reviews, averageRating) = await movieService.GetMovieDetailsAsync(imdbID);

        // Assert: Verifiera att metoden returnerar standardv�rden n�r ett undantag intr�ffar
        Assert.NotNull(movie); // Filmen ska inte vara null
        Assert.Equal(string.Empty, movie.Title); // Filmtiteln ska vara en tom str�ng som standard
        Assert.Empty(reviews); // Inga recensioner ska returneras
        Assert.Equal(0, averageRating); // Medelv�rdet ska vara 0
    }
}
