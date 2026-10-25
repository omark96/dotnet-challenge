# Candidate handoff

Keep this short. Record what you actually observed and verified; unfinished work is expected within the timebox.



## Priorities and reproduction evidence

[What did you prioritize and why? Include steps, example input, expected versus observed behavior, and any useful evidence.]

### “I searched for `anna` and couldn't find Anna Lind.”
I began by reproducing the bug, by searching for "anna" and no user was found. I then searched for "Anna" to verify she existed in the data.
I then added a new test case to `GetUsersAsync_FindsNameMatchesAndTrimsSearch` to check for lower case search as well. The test now failed.
Then I followed the test to the failing method and noticed as I expected that it was doing a case sensitive contains.

### “The surnames don't appear in the order our Swedish team expects.”
I verified the bug in the browser and "Linnea Äberg" came before "Erik Åström" in the list.
I then had AI generate a new test: `GetUsersAsync_SortsBySwedishAlphabet` and the test now failed

### “I clicked Deactivate and wasn't asked to confirm. I also couldn't tell whether anything had happened until I refreshed the page.”
I opened the website and verified that no confirmation was asked when click on deactivate. I also confirmed that nothing updated automatically,
but a manual refresh updated to the correct status.

## Changes

[What changed, and how does it help the administrator?]

### “I searched for `anna` and couldn't find Anna Lind.”
I changed UserService.GetUsersAsync() to convert the search term and both the first and last name of the person to lower case before comparison.
This now lets the administrator search for people case insensitive.

### “The surnames don't appear in the order our Swedish team expects.”
UserService.GetUsersAsync() now sorts the list of users by name in memory to be able to use Swedish ordering rules. This makes it easier for the admin
to be able to find people in the list.

### “I clicked Deactivate and wasn't asked to confirm. I also couldn't tell whether anything had happened until I refreshed the page.”
Now the page prompts the user through a confirm window both when activating and deactivating a user. The list also refreshes when a user's status is changed.
This makes it easier for the admin to tell that the changes has actually taken place and also makes it harder to accidentally activate/deactivate a user.

### FEATURE-104 – Filter users by status
The admin is now able to also filter by user status when searching for users. There is a dropdown next to the original search bar.
Added a new UserStatusFilter enum that is used to decide what filter to apply when searching for a user. UserService.GetUsersAsync() got that as a new parameter.
A new test was also added to ensure that different combinations of status and search term work.

## Verification

[List the commands or manual checks you ran and their actual results. Include regression coverage and anything you could not verify.]

### “I searched for `anna` and couldn't find Anna Lind.”

`dotnet test .\usermanager.sln` before changes:
Test summary: total: 41; failed: 0; succeeded: 41; skipped: 0; duration: 2,9s
`dotnet test .\usermanager.sln` after changes:
Test summary: total: 42; failed: 0; succeeded: 42; skipped: 0; duration: 2,8s

### “The surnames don't appear in the order our Swedish team expects.”

`dotnet test.\usermnager.sln` before changes:
Test summary: total: 42; failed: 0; succeeded: 42; skipped: 0; duration: 2,8s
```
Expected: string[]                         ["Ahlström", "Zetterberg", "Åberg", "Ängström", "Öberg"]
Actual:   ListSelectIterator<User, string> ["Ahlström", "Zetterberg", "Ängström", "Åberg", "Öberg"]
```

`dotnet test.\usermanager.sln` after changes:
`Test summary: total: 43; failed: 0; succeeded: 43; skipped: 0; duration: 3,1s`

### FEATURE-104 – Filter users by status
`dotnet test.\usermnager.sln` before changes:
Test summary: total: 43; failed: 0; succeeded: 43; skipped: 0; duration: 3,4s
`dotnet test.\usermnager.sln` after changes:
Test summary: total: 49; failed: 0; succeeded: 49; skipped: 0; duration: 3,1s

## Unfinished work

[What remains, known limitations, and the most useful next step.]
### “I searched for `anna` and couldn't find Anna Lind.”:

### “The surnames don't appear in the order our Swedish team expects.”

See "Decisions and trade-offs" below for this bug. Currently there are features that would be possible on something like Sql Server/Postgres by setting collation,
but if production is running on something other than sqlite we now have two different behaviours in the tests and production.

### “I clicked Deactivate and wasn't asked to confirm. I also couldn't tell whether anything had happened until I refreshed the page.”

The confirm popup is the default browser popup, it might be worth upgrading it to a proper modal to better match the styling of the rest of the website.

### FEATURE-104 – Filter users by status

A new parameter was simply added to the GetUsersAsync method and so far it seems good, however if we add the ability to filter by creation date for example 
now we get two additional parameters. If the list of parameters starts to grow it might be worth considering moving to something like a UserSearchModel to avoid an
explosion of parameters to that method. For now I think it still works as is though.

### Unfinished
“I changed a user's email address and got a saved message, but when I opened the record again the old address was still there.” and 
“I imported the same CSV twice and now I can see the same people more than once.” both remain unfinished. I began work on the CSV bug, but realized I had to reprioritize
to have enough time to implement the requested feature as well as answer the customer request.

## Decisions and trade-offs

### “I searched for `anna` and couldn't find Anna Lind.”
I first tried adding a StringComparison.OrdinalIgnoreCase as a second comparison to the contains as per [microsoft's recommendation](https://learn.microsoft.com/en-us/dotnet/standard/base-types/best-practices-strings#recommendations-for-string-usage).
However, EF Core failed to convert that to a query, so I instead opted to use ToLower() on them instead before doing the comparison.

### “The surnames don't appear in the order our Swedish team expects.”
Since the test database is using sqlite I am not able to use approaches such as setting a correct collation on the database to give the correct ordering automatically.
Currently I convert the result into a list and do the ordering in memory, which works on this solution for now, but will lead to unnecessarily loading all users into memory
if we add things like pagination in the future.

### “I clicked Deactivate and wasn't asked to confirm. I also couldn't tell whether anything had happened until I refreshed the page.”
I assumed the administrator also wanted a confirmation behaviour for activating a user, if this had been a real case I would have first double checked if this was the wanted behaviour.
I considered building a modal for the page to have better styling, but found the scope to be outside what is achievable in this timeframe.

### FEATURE-104 – Filter users by status
I first considered using a similar pattern as the search term with a `?bool isActive`, but an enum more cleanly maps toward a select dropdown and is what I ended up using.
I also did consider adding the UserSearchModel, but decided that in the current state it was not necessary, but as stated, if more filter fields are added it might be a good future change.

## AI use and checking

### “I searched for `anna` and couldn't find Anna Lind.”
Not used

### “The surnames don't appear in the order our Swedish team expects.”
I gave it a sample from one of the previous tests, the ValidModel method and asked it to generate me a test case for ensuring that Swedish last name sorting works.
I checked the output towards previously written tests to ensure it kept a similar style.

I also used it to verify different possible solutions after I had gotten my own solution, but since the test server is running sqlite I was not able to implement them.

### “I clicked Deactivate and wasn't asked to confirm. I also couldn't tell whether anything had happened until I refreshed the page.”
Not used, I used a similar solution to what I have recently done in a project on my own.

### FEATURE-104 – Filter users by status

Similarly to “The surnames don't...” I chose to let the AI generate a test case based on previous test cases. It initially suggested adding yet another method along the
lines of ValidModelWithStatus, but as that essentially became just the same as what is needed for just using the constructor I opted to follow the pattern of the rest 
of the tests instead with using the constructor and adding them to the context one by one.

I also had Claude generate the select for the Home-page to be shown next to the search field.

