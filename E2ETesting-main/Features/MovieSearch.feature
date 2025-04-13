@search
Feature: MovieSearch

Testar att söka på filmer, sedan testar jag att söka på filmer utan att söka på något alls och sedan genom att skriva jättemycket


Scenario Outline: Searching for movie whilst being logged out
	Given I am on the homepage logged out
	When I enter "<SearchLine>" in the search bar
	And I press the search button
	Then I should see a list of movies matching "<SearchLine>"

Examples: 
	| SearchLine |
	| Avatar     |
	| Titanic    |
	| Star Wars  |
	| Inception  |
	| The Matrix |

Scenario: Searching for movie without entering anything
	Given I am on the homepage logged out
	When I enter "" in the search bar
	And I press the search button
	Then I should see a message saying "Fyll i det här fältet."

Scenario: Searching for a movie with nonsense input
    Given I am on the homepage logged out
    When I enter "asdfghjkl123!@#" in the search bar
    And I press the search button
    Then I should see a message saying "Inga filmer hittades för "asdfghjkl123!@#". Prova igen!" 
