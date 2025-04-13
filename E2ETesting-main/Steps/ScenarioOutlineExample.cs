namespace E2ETesting.Steps;

using Microsoft.Playwright;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Reflection.PortableExecutable;
using TechTalk.SpecFlow;
using Xunit;

[Binding]
public class SimpleFormSteps
{
    private IPlaywright _playwright;
    private IBrowser _browser;
    private IBrowserContext _context;
    private IPage _page;

    [BeforeScenario]
    public async Task Setup()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new() { Headless = false, SlowMo = 200 });
        _context = await _browser.NewContextAsync();
        _page = await _context.NewPageAsync();
    }

    [AfterScenario]
    public async Task Teardown()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    [Given("I am on the homepage")]
    public async Task GivenIAmOnTheHomePage()
    {
        await _page.GotoAsync("https://localhost:7295/Movies");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle); // Vänta på att sidan ska ladda klart
    }

    [When("I press the login button")]
    public async Task WhenIPressTheLoginButton()
    {
        await _page.ClickAsync("a[href='/Identity/Account/Login']");
    }

    [When(@"I enter ""(.*)"" as the username")]
    public async Task WhenIEnterAsTheUsername(string username)
    {
        await _page.FillAsync("input[name='Input.Email']", username);
    }

    [When(@"I enter ""(.*)"" as the password")]
    public async Task WhenIEnterAsThePassword(string password)
    {
        await _page.FillAsync("input[name='Input.Password']", password);
    }

    [When("I press the logga in button")]
    public async Task WhenIPressTheLoggaInButton()
    {
        await _page.ClickAsync("#login-submit");
    }

    [When("I press the mina recensioner button")]
    public async Task WhenIPressTheMinaRecensionerButton()
    {
        await _page.ClickAsync("a[href='/Identity/User']");
    }


    [When("I press the Mowi Tajm logo")]
    public async Task WhenIPressTheMowiTajmLogo()
    {
        await _page.ClickAsync("img[alt='MowiTajm']");
    }

    [When("I press the logout button")]
    public async Task WhenIPressTheLogoutButton()
    {
        await _page.ClickAsync("nav button.btn-navbar:has-text('Logga ut')");
    }

    [When("I press the confirm logout button")]
    public async Task WhenIPressTheConfirmLogoutButton()
    {
        _page.Dialog += async (_, dialog) =>
        {
            if (dialog.Type == "confirm")
            {
                await dialog.AcceptAsync(); // Tryck på "OK"
            }
        };

        await _page.ClickAsync("form[action*='/Account/Logout'] button:has-text('Logga ut')");
    }

    [Then("I should be back on the homepage and be logged out")]
    public async Task ThenIShouldBeBackOnTheHomePageAndBeLoggedOut()
    {

        await Task.Delay(5000);
        // Vänta på att rubriken "Välkommen till MowiTajm!" ska synas
        await _page.WaitForSelectorAsync("h1.text-center");

        // Hämta texten från rubriken
        var heading = await _page.InnerTextAsync("h1.text-center");

        // Kontrollera att texten matchar
        Assert.Equal("Välkommen till MowiTajm!", heading);
    }

    [When(@"I should see an error message saying ""(.*)""")]
    public async Task WhenIShouldSeeAnErrorMessageSaying(string expectedMessage)
    {
        var errorMessageSelector = "div.validation-summary-errors ul li";
        var errorMessage = await _page.InnerTextAsync(errorMessageSelector);
        Assert.Equal(expectedMessage, errorMessage);
    }

    [Then("I should still be on the login page")]
    public async Task ThenIShouldStillBeOnTheLoginPage()
    {
        Assert.Contains("/Identity/Account/Login", _page.Url);
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //Movie search
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    [Given("I am on the homepage logged out")]
    public async Task GivenIAmOnTheHomePageLoggedOut()
    {
        await _page.GotoAsync("https://localhost:7295/Movies");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle); // Vänta på att sidan ska ladda klart
    }

    [When(@"I enter ""(.*)"" in the search bar")]
    public async Task WhenIEnterInTheSearchBar(string SearchInput)
    {
        await _page.FillAsync("input[name='searchInput']", SearchInput);
    }

    [When("I press the search button")]
    public async Task WhenIPressTheSearchButton()
    {
        await _page.ClickAsync("button.btn-search");
    }

    [Then(@"I should see a list of movies matching ""(.*)""")]
    public async Task ThenIShouldSeeAListOfMoviesMatching(string searchTerm)
    {
        // Hämta alla filmtitlar från sökresultaten
        var movieTitles = await _page.Locator(".search-result-title").AllInnerTextsAsync();

        // Kontrollera att alla filmer innehåller sökordet
        foreach (var title in movieTitles)
        {
            // Kontrollera om titeln innehåller sökordet (case insensitive)
            Assert.True(title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase),
                $"Movie title '{title}' does not match the search term '{searchTerm}'.");
        }
    }

    [Then(@"I should see a message saying ""(.*)""")]
    public async Task ThenIShouldSeeAMessageSaying(string expectedMessage)
    {
        if (expectedMessage == "Fyll i det här fältet.")
        {
            var input = await _page.QuerySelectorAsync("input[name='searchInput']");
            var validationMessage = await input.EvaluateAsync<string>("el => el.validationMessage");
            Assert.Equal(expectedMessage, validationMessage);
        }
        else
        {
            await _page.WaitForSelectorAsync("#noResultMessage");
            var actualMessage = await _page.InnerTextAsync("#noResultMessage");
            Assert.Equal(expectedMessage, actualMessage.Trim());
        }
    }









    [When("I press the register button")]
    public async Task WhenIPressTheRegisterButton()
    {
        await _page.ClickAsync("a.btn-navbar:has-text('Registrera konto')");
    }

    [Then("I should be on the register page")]
    public async Task ThenIShouldBeOnTheRegisterPage()
    {
        var heading = await _page.TextContentAsync("h1.account-title");
        Assert.Equal("Registrera Konto", heading);
    }

    //    Given I am on the Admin Page
    [Given("I am on the Admin Page")]
    public async Task GivenIAmOnTheAdminPage()
    {
        await _page.GotoAsync("https://localhost:7295/Identity/Admin");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle); // Vänta på att sidan ska ladda klart
    }
    //And I see the Recensioner List
    [When("I press the Hantera recensioner button")]
    public async Task WhenIPressTheHanteraRecensionerButton()
    { 
        await _page.ClickAsync("button:has-text('Hantera recensioner')");
    }
    //When I select "<dropdown>" from the rating filter
    [When(@"I select ""(.*)"" from the rating filter")]
    public async Task WhenISelectFromTheRatingFilter(string rating)
    {
        await _page.SelectOptionAsync("#ratingFilter", rating);
    }
    //    Then I should see reviews with "<rating>"
    [Then(@"I should see reviews with ""(.*)""")]
    public async Task ThenIShouldSeeReviewsWith(string expectedRating)
    {
        var ratings = await _page.Locator(".review .rating").AllInnerTextsAsync();

        foreach (var rating in ratings)
        {
            Assert.Equal(expectedRating, rating);
        }
    }
}

