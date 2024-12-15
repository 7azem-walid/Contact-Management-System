module Program

open System
open System.Windows.Forms
open System.Drawing
open System.Text.RegularExpressions

// Define the Contact type
type Contact =
    { Id: int
      Name: string
      PhoneNumber: string
      Email: string }

// Contact storage
let mutable contacts = []
let mutable idCounter = 1

// Helper function to display contacts
let displayContacts (listBox: ListBox) =
    listBox.Items.Clear()
    for contact in contacts do
        listBox.Items.Add($"ID: {contact.Id}, Name: {contact.Name}, Phone: {contact.PhoneNumber}, Email: {contact.Email}") |> ignore

// Define the InputBox function
let InputBox (prompt: string, title: string) =
    let form = new Form(Text = title, Size = Size(400, 150), FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false, StartPosition = FormStartPosition.CenterParent)
    let label = new Label(Text = prompt, Dock = DockStyle.Top, Height = 30)
    let textBox = new TextBox(Dock = DockStyle.Top)
    let okButton = new Button(Text = "OK", DialogResult = DialogResult.OK, Dock = DockStyle.Right, Width = 80)
    let cancelButton = new Button(Text = "Cancel", DialogResult = DialogResult.Cancel, Dock = DockStyle.Right, Width = 80)
    
    // Add controls to the form
    form.Controls.AddRange([| label :> Control; textBox :> Control; okButton :> Control; cancelButton :> Control |])
    
    // Set accept and cancel buttons
    form.AcceptButton <- okButton
    form.CancelButton <- cancelButton

    // Show the form and return the result
    match form.ShowDialog() with
    | DialogResult.OK -> textBox.Text
    | _ -> ""

// Function to get numeric input (e.g., phone number) and ensure it's at least 11 digits long
let rec getNumericInput (prompt: string, title: string) =
    let input = InputBox(prompt, title)
    if input <> "" && input |> Seq.forall Char.IsDigit && input.Length >= 11 then
        input
    else
        MessageBox.Show("Please enter a valid numeric value with at least 11 digits.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning) |> ignore
        getNumericInput(prompt, title)

// Function to validate email address format
let isValidEmail (email: string) =
    let emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zAZ0-9.-]+\.[a-zA-Z]{2,}$"
    Regex.IsMatch(email, emailPattern)

// Function to get valid email input
let rec getEmailInput (prompt: string, title: string) =
    let email = InputBox(prompt, title)
    if isValidEmail(email) then
        email
    else
        MessageBox.Show("Please enter a valid email address.", "Invalid Email", MessageBoxButtons.OK, MessageBoxIcon.Warning) |> ignore
        getEmailInput(prompt, title)

[<STAThread>]
let main () =
    // Create the form
    let form = new Form(Text = "Contact Manager", Size = Size(600, 400))

    // Create controls
    let listBox = new ListBox(Dock = DockStyle.Top, Height = 200)
    let addButton = new Button(Text = "Add", Width = 80)
    let editButton = new Button(Text = "Edit", Width = 80)
    let deleteButton = new Button(Text = "Delete", Width = 80)
    let searchButton = new Button(Text = "Search", Width = 80)
    let quitButton = new Button(Text = "Quit", Width = 80)

    // Create a panel to hold buttons at the bottom
    let buttonPanel = new FlowLayoutPanel(Dock = DockStyle.Bottom, Height = 50)
    buttonPanel.Controls.AddRange([| addButton :> Control
                                     editButton :> Control
                                     deleteButton :> Control
                                     searchButton :> Control
                                     quitButton :> Control |])

    // Add controls to the form
    form.Controls.AddRange([| listBox :> Control; buttonPanel :> Control |])

    // Event Handlers
    addButton.Click.Add(fun _ ->
        let name = InputBox("Enter the contact's name:", "Add Contact")
        let phoneNumber = getNumericInput("Enter the contact's phone number (digits only, at least 11 digits):", "Add Contact")
        let email = getEmailInput("Enter the contact's email address:", "Add Contact")
        if name <> "" && phoneNumber <> "" && email <> "" then
            let newContact = { Id = idCounter; Name = name; PhoneNumber = phoneNumber; Email = email }
            contacts <- newContact :: contacts
            idCounter <- idCounter + 1
            displayContacts listBox
    )

    editButton.Click.Add(fun _ ->
        // Ask the user whether they want to edit by name or phone number
        let editCriteria = InputBox("Edit by (1) Name or (2) Phone Number:", "Edit Contact")
        
        if editCriteria = "1" then
            // Edit by Name
            let searchName = InputBox("Enter the contact name to edit:", "Edit by Name")
            if searchName <> "" then
                match contacts |> List.tryFind (fun c -> c.Name.Equals(searchName, StringComparison.OrdinalIgnoreCase)) with
                | Some contact ->
                    let newName = InputBox("Enter the new contact name:", "Edit Contact")
                    let newPhoneNumber = getNumericInput("Enter the new contact phone number (digits only, at least 11 digits):", "Edit Contact")
                    let newEmail = getEmailInput("Enter the new contact email address:", "Edit Contact")
                    contacts <- contacts |> List.map (fun c -> if c.Id = contact.Id then { c with Name = newName; PhoneNumber = newPhoneNumber; Email = newEmail } else c)
                    displayContacts listBox
                | None -> MessageBox.Show("Contact not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) |> ignore
        elif editCriteria = "2" then
            // Edit by Phone Number
            let searchPhoneNumber = InputBox("Enter the contact phone number to edit:", "Edit by Phone Number")
            if searchPhoneNumber <> "" then
                match contacts |> List.tryFind (fun c -> c.PhoneNumber.Equals(searchPhoneNumber)) with
                | Some contact ->
                    let newName = InputBox("Enter the new contact name:", "Edit Contact")
                    let newPhoneNumber = getNumericInput("Enter the new contact phone number (digits only, at least 11 digits):", "Edit Contact")
                    let newEmail = getEmailInput("Enter the new contact email address:", "Edit Contact")
                    contacts <- contacts |> List.map (fun c -> if c.Id = contact.Id then { c with Name = newName; PhoneNumber = newPhoneNumber; Email = newEmail } else c)
                    displayContacts listBox
                | None -> MessageBox.Show("Contact not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) |> ignore
        else
            MessageBox.Show("Invalid option, please enter '1' for Name or '2' for Phone Number.", "Invalid Edit Option", MessageBoxButtons.OK, MessageBoxIcon.Warning) |> ignore
    )

    deleteButton.Click.Add(fun _ ->
        // Ask the user whether they want to delete by name or phone number
        let deleteCriteria = InputBox("Delete by (1) Name or (2) Phone Number:", "Delete Contact")
        
        if deleteCriteria = "1" then
            // Delete by Name
            let searchName = InputBox("Enter the contact name to delete:", "Delete by Name")
            if searchName <> "" then
                match contacts |> List.tryFind (fun c -> c.Name.Equals(searchName, StringComparison.OrdinalIgnoreCase)) with
                | Some contact ->
                    contacts <- contacts |> List.filter (fun c -> c.Id <> contact.Id)
                    displayContacts listBox
                    MessageBox.Show($"Contact {searchName} deleted!", "Contact Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information) |> ignore
                | None -> MessageBox.Show("Contact not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) |> ignore
        elif deleteCriteria = "2" then
            // Delete by Phone Number
            let searchPhoneNumber = InputBox("Enter the contact phone number to delete:", "Delete by Phone Number")
            if searchPhoneNumber <> "" then
                match contacts |> List.tryFind (fun c -> c.PhoneNumber.Equals(searchPhoneNumber)) with
                | Some contact ->
                    contacts <- contacts |> List.filter (fun c -> c.Id <> contact.Id)
                    displayContacts listBox
                    MessageBox.Show($"Contact with phone number {searchPhoneNumber} deleted!", "Contact Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information) |> ignore
                | None -> MessageBox.Show("Contact not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) |> ignore
        else
            MessageBox.Show("Invalid option, please enter '1' for Name or '2' for Phone Number.", "Invalid Delete Option", MessageBoxButtons.OK, MessageBoxIcon.Warning) |> ignore
    )

    searchButton.Click.Add(fun _ ->
        // Ask the user whether they want to search by name or phone number
        let searchCriteria = InputBox("Search by (1) Name or (2) Phone Number:", "Search Contact")
        
        if searchCriteria = "1" then
            let searchName = InputBox("Enter the contact name to search:", "Search by Name")
            if searchName <> "" then
                let foundContacts = contacts |> List.filter (fun c -> c.Name.IndexOf(searchName, StringComparison.OrdinalIgnoreCase) >= 0)
                listBox.Items.Clear()
                if List.isEmpty foundContacts then
                    listBox.Items.Add("No contacts found!") |> ignore
                else
                    for contact in foundContacts do
                        listBox.Items.Add($"ID: {contact.Id}, Name: {contact.Name}, Phone: {contact.PhoneNumber}, Email: {contact.Email}") |> ignore
        elif searchCriteria = "2" then
            let searchPhoneNumber = InputBox("Enter the contact phone number to search:", "Search by Phone Number")
            if searchPhoneNumber <> "" then
                let foundContacts = contacts |> List.filter (fun c -> c.PhoneNumber.IndexOf(searchPhoneNumber) >= 0)
                listBox.Items.Clear()
                if List.isEmpty foundContacts then
                    listBox.Items.Add("No contacts found!") |> ignore
                else
                    for contact in foundContacts do
                        listBox.Items.Add($"ID: {contact.Id}, Name: {contact.Name}, Phone: {contact.PhoneNumber}, Email: {contact.Email}") |> ignore
        else
            MessageBox.Show("Invalid option, please enter '1' for Name or '2' for Phone Number.", "Invalid Search Option", MessageBoxButtons.OK, MessageBoxIcon.Warning) |> ignore
    )

    quitButton.Click.Add(fun _ -> form.Close())

    // Run the application
    Application.Run(form)

// Entry point
main ()
















