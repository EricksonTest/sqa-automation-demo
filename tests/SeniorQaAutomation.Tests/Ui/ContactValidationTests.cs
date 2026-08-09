using NUnit.Framework;
using SeniorQaAutomation.Framework.PageObjects;

namespace SeniorQaAutomation.Tests.Ui;

[TestFixture]
[Category("Ui")]
public sealed class ContactValidationTests : UiTestBase
{
    [Test]
    public void ContactForm_ShowsValidationErrorsWhenRequiredFieldsAreEmpty()
    {
        var homePage = new HomePage(Driver, Settings.BaseUrl, Settings.UiTimeout)
            .Open()
            .SubmitEmptyContactForm();

        Assert.That(
            () => homePage.ContactValidationErrors,
            Is.Not.Empty,
            $"Expected server-side validation feedback for an empty contact message.");
    }
}
