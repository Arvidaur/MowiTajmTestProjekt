@loginAndNavigation

Feature: Login attempts and navigation

loggar in med två existerande användare och testar sedan att logga in med fel användarinformation och testar att navigera runt på sidan samt testar att sortera betyg på admin sidan

Scenario Outline: Successful login and logout
    Given I am on the homepage
    When I press the login button
    And I enter "<username>" as the username
    And I enter "<password>" as the password
    And I press the logga in button
    And I press the mina recensioner button
    And I press the Mowi Tajm logo
    And I press the logout button
    And I press the confirm logout button
    Then I should be back on the homepage and be logged out

    Examples: 
    | username         | password   |
    | arvid@gmail      | Arvid123!  |
    | donkey@kong      | Donkey123! |
	

Scenario Outline: Failed login with invalid credentials
  Given I am on the homepage
  When I press the login button
  And I enter "<username>" as the username
  And I enter "<password>" as the password
  And I press the logga in button
  And I should see an error message saying "Invalid login attempt."
  Then I should still be on the login page

  Examples:
    | username         | password     |
    | invalid@username | Invalid123!  |
    | test@wrong.com   | Wrongpass123 |


Scenario: Navigate to the register page
    Given I am on the homepage
    When I press the register button
    Then I should be on the register page

Scenario Outline: Sortera betyg på Hantera recenisoner sidan
    Given I am on the Admin Page
    When I press the Hantera recensioner button
    And I select "<dropdown>" from the rating filter
    Then I should see reviews with "<rating>"

Examples: 
    | dropdown | rating |
    | 5        | 3      |
    | 2        | 1      |
    | 3        | 0      | 


