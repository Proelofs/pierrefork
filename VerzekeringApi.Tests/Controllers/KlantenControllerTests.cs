
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using VerzekeringApi.Controllers;
using VerzekeringApi.Data;
using VerzekeringApi.Dtos;
using VerzekeringApi.Models;
using VerzekeringApi.Tests.TestHelpers;

namespace VerzekeringApi.Tests.Controllers;

[TestFixture]
public class KlantenControllerTests
{
    private static KlantenController CreateController(AppDbContext db) => new(db);

    [Test]
    public async Task CreateKlant_ReturnsCreated_AndSetsBeginDatum()
    {
        // Arrange
        using var db = DbTestUtils.CreateInMemoryDbContext();
        var controller = CreateController(db);
        var dto = new CreateKlantDto
        {
            Voornaam = "Jan",
            Tussenvoegsel = "van",
            Achternaam = "Dijk",
            Geboortedatum = new DateTime(1985, 3, 21),
            Woonplaats = "Zwolle"
        };

        // Act
        var result = await controller.CreateKlant(dto);

        // Assert
        var created = result as CreatedAtActionResult;
        Assert.That(created, Is.Not.Null, "Expected CreatedAtActionResult");

        var klant = created!.Value as Klant;
        Assert.That(klant, Is.Not.Null);
        Assert.That(klant!.Voornaam, Is.EqualTo("Jan"));
        Assert.That(klant.BeginDatum, Is.Not.EqualTo(default(DateTime)));
        Assert.That(klant.EindDatum, Is.Null);

        var inDb = await db.Klanten.FirstOrDefaultAsync(k => k.Id == klant.Id);
        Assert.That(inDb, Is.Not.Null);
    }

    [Test]
    public async Task GetKlantById_ReturnsNotFound_WhenMissing()
    {
        using var db = DbTestUtils.CreateInMemoryDbContext();
        var controller = CreateController(db);

        var result = await controller.GetKlantById(Guid.NewGuid());

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task SetEinddatum_SetsNow_WhenNullInDto()
    {
        using var db = DbTestUtils.CreateInMemoryDbContext();
        var klant = new Klant
        {
            Voornaam = "Piet",
            Achternaam = "Jansen",
            Geboortedatum = new DateTime(1990, 1, 1),
            Woonplaats = "Zwolle",
            BeginDatum = DateTime.Now
        };
        db.Klanten.Add(klant);
        await db.SaveChangesAsync();

        var controller = CreateController(db);

        var result = await controller.SetEinddatum(klant.Id, new SetEinddatumDto { EindDatum = null });

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);

        var updated = ok!.Value as Klant;
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.EindDatum, Is.Not.Null);

        var inDb = await db.Klanten.FindAsync(klant.Id);
        Assert.That(inDb!.EindDatum, Is.Not.Null);
    }

    [Test]
    public async Task SetEinddatum_ReturnsBadRequest_WhenAlreadySet()
    {
        using var db = DbTestUtils.CreateInMemoryDbContext();
        var klant = new Klant
        {
            Voornaam = "Kees",
            Achternaam = "Bakker",
            Geboortedatum = new DateTime(1980, 6, 6),
            Woonplaats = "Zwolle",
            BeginDatum = DateTime.Now,
            EindDatum = DateTime.Now.AddDays(-1)
        };
        db.Klanten.Add(klant);
        await db.SaveChangesAsync();

        var controller = CreateController(db);

        var result = await controller.SetEinddatum(klant.Id, new SetEinddatumDto { EindDatum = DateTime.Now });

        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest, Is.Not.Null);
        Assert.That(badRequest!.Value?.ToString(), Does.Contain("al een einddatum"));
    }
}
