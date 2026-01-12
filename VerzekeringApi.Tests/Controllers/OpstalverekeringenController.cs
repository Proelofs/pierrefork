
using System;
using System.Collections.Generic;
using System.Linq;
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
public class OpstalverzekeringenControllerTests
{
    private static OpstalverzekeringenController CreateController(AppDbContext db) => new(db);

    private static async Task<Klant> SeedActiveKlant(AppDbContext db)
    {
        var klant = new Klant
        {
            Voornaam = "Anna",
            Achternaam = "De Boer",
            Geboortedatum = new DateTime(1992, 2, 2),
            Woonplaats = "Zwolle",
            BeginDatum = DateTime.Now
        };
        db.Klanten.Add(klant);
        await db.SaveChangesAsync();
        return klant;
    }

    [Test]
    public async Task CreateOpstalverzekering_ReturnsCreated_AndPersists()
    {
        using var db = DbTestUtils.CreateInMemoryDbContext();
        var klant = await SeedActiveKlant(db);
        var controller = CreateController(db);

        var dto = new CreateOpstalverzekeringDto
        {
            PolisNummer = 1001,
            KlantId = klant.Id,
            TypeDekking = "All-risk",
            GedekteGebeurtenissen = "Brand, storm",
            Uitsluitingen = "Achterstallig onderhoud",
            Herbouwwaarde = 350000m,
            Inboedelwaarde = 50000m,
            Premie = 45.99m,
            Betaaltermijn = "Maandelijks",
            AanvullendeOpties = "Glasverzekering"
        };

        var result = await controller.CreateOpstalverzekering(dto);

        var created = result as CreatedAtActionResult;
        Assert.That(created, Is.Not.Null);

        var ov = created!.Value as Opstalverzekering;
        Assert.That(ov, Is.Not.Null);
        Assert.That(ov!.PolisNummer, Is.EqualTo(1001));
        Assert.That(ov.KlantId, Is.EqualTo(klant.Id));
        Assert.That(ov.TypeDekking, Is.EqualTo("All-risk"));
        Assert.That(ov.BeginDatum, Is.Not.EqualTo(default(DateTime)));
        Assert.That(ov.EindDatum, Is.Null);

        var inDb = await db.Opstalverzekeringen.FirstOrDefaultAsync(o => o.Id == ov.Id);
        Assert.That(inDb, Is.Not.Null);
    }

    [Test]
    public async Task CreateOpstalverzekering_ReturnsBadRequest_WhenKlantNotActive()
    {
        using var db = DbTestUtils.CreateInMemoryDbContext();

        var klant = new Klant
        {
            Voornaam = "Tom",
            Achternaam = "Stop",
            Geboortedatum = new DateTime(1990, 1, 1),
            Woonplaats = "Zwolle",
            BeginDatum = DateTime.Now,
            EindDatum = DateTime.Now
        };
        db.Klanten.Add(klant);
        await db.SaveChangesAsync();

        var controller = CreateController(db);

        var dto = new CreateOpstalverzekeringDto
        {
            PolisNummer = 2002,
            KlantId = klant.Id,
            TypeDekking = "Basis",
            Herbouwwaarde = 100000m,
            Inboedelwaarde = 20000m,
            Premie = 20m,
            Betaaltermijn = "Maandelijks"
        };

        var result = await controller.CreateOpstalverzekering(dto);

        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest, Is.Not.Null);
        Assert.That(badRequest!.Value?.ToString(), Does.Contain("beëindigd"));
    }

    [Test]
    public async Task GetByPolisNummer_ReturnsOnlyActive()
    {
        using var db = DbTestUtils.CreateInMemoryDbContext();
        var klant = await SeedActiveKlant(db);
        var controller = CreateController(db);

        var active = new Opstalverzekering
        {
            PolisNummer = 3003,
            KlantId = klant.Id,
            TypeDekking = "All-risk",
            Herbouwwaarde = 300000m,
            Inboedelwaarde = 40000m,
            Premie = 40m,
            Betaaltermijn = "Maandelijks",
            BeginDatum = DateTime.Now
        };
        var ended = new Opstalverzekering
        {
            PolisNummer = 3003, // note: in real DB this would violate unique index.
            KlantId = klant.Id,
            TypeDekking = "All-risk",
            Herbouwwaarde = 300000m,
            Inboedelwaarde = 40000m,
            Premie = 40m,
            Betaaltermijn = "Maandelijks",
            BeginDatum = DateTime.Now.AddMonths(-6),
            EindDatum = DateTime.Now.AddDays(-1)
        };

        db.Opstalverzekeringen.AddRange(active, ended);
        await db.SaveChangesAsync();

        var result = await controller.GetByPolisNummer(3003);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);

        var ov = ok!.Value as Opstalverzekering;
        Assert.That(ov, Is.Not.Null);
        Assert.That(ov!.EindDatum, Is.Null);
    }

    [Test]
    public async Task GetByKlant_ReturnsOnlyActive()
    {
        using var db = DbTestUtils.CreateInMemoryDbContext();
        var klant = await SeedActiveKlant(db);
        var controller = CreateController(db);

        db.Opstalverzekeringen.Add(new Opstalverzekering
        {
            PolisNummer = 4004,
            KlantId = klant.Id,
            TypeDekking = "Basis",
            Herbouwwaarde = 100000m,
            Inboedelwaarde = 10000m,
            Premie = 15m,
            Betaaltermijn = "Maandelijks",
            BeginDatum = DateTime.Now
        });
        db.Opstalverzekeringen.Add(new Opstalverzekering
        {
            PolisNummer = 4005,
            KlantId = klant.Id,
            TypeDekking = "Basis",
            Herbouwwaarde = 100000m,
            Inboedelwaarde = 10000m,
            Premie = 15m,
            Betaaltermijn = "Maandelijks",
            BeginDatum = DateTime.Now.AddMonths(-12),
            EindDatum = DateTime.Now.AddDays(-2)
        });
        await db.SaveChangesAsync();

        var result = await controller.GetByKlant(klant.Id);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);

        var list = ok!.Value as IEnumerable<Opstalverzekering>;
        Assert.That(list, Is.Not.Null);
        Assert.That(list!.Count(), Is.EqualTo(1));
        Assert.That(list.All(o => o.EindDatum is null), Is.True);
    }

    [Test]
    public async Task GetByTypeDekking_ReturnsOnlyActive_ExactMatch()
    {
        using var db = DbTestUtils.CreateInMemoryDbContext();
        var klant = await SeedActiveKlant(db);
        var controller = CreateController(db);

        db.Opstalverzekeringen.AddRange(
            new Opstalverzekering
            {
                PolisNummer = 5001,
                KlantId = klant.Id,
                TypeDekking = "All-risk",
                Herbouwwaarde = 150000m,
                Inboedelwaarde = 15000m,
                Premie = 25m,
                Betaaltermijn = "Maandelijks",
                BeginDatum = DateTime.Now
            },
            new Opstalverzekering
            {
                PolisNummer = 5002,
                KlantId = klant.Id,
                TypeDekking = "All-Risk", // different case; exact-match should not include
                Herbouwwaarde = 150000m,
                Inboedelwaarde = 15000m,
                Premie = 25m,
                Betaaltermijn = "Maandelijks",
                BeginDatum = DateTime.Now
            },
            new Opstalverzekering
            {
                PolisNummer = 5003,
                KlantId = klant.Id,
                TypeDekking = "All-risk",
                Herbouwwaarde = 150000m,
                Inboedelwaarde = 15000m,
                Premie = 25m,
                Betaaltermijn = "Maandelijks",
                BeginDatum = DateTime.Now.AddMonths(-3),
                EindDatum = DateTime.Now.AddDays(-1)
            }
        );
        await db.SaveChangesAsync();

        var result = await controller.GetByTypeDekking("All-risk");

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);

        var list = ok!.Value as IEnumerable<Opstalverzekering>;
        Assert.That(list, Is.Not.Null);
        Assert.That(list!.Count(), Is.EqualTo(1));
        Assert.That(list!.All(o => o.TypeDekking == "All-risk"), Is.True);
        Assert.That(list!.All(o => o.EindDatum is null), Is.True);
    }

    [Test]
    public async Task SetEinddatum_SetsNow_WhenNullInDto()
    {
        using var db = DbTestUtils.CreateInMemoryDbContext();
        var klant = await SeedActiveKlant(db);
        var controller = CreateController(db);

        var ov = new Opstalverzekering
        {
            PolisNummer = 6006,
            KlantId = klant.Id,
            TypeDekking = "Basis",
            Herbouwwaarde = 200000m,
            Inboedelwaarde = 20000m,
            Premie = 20m,
            Betaaltermijn = "Maandelijks",
            BeginDatum = DateTime.Now
        };
        db.Opstalverzekeringen.Add(ov);
        await db.SaveChangesAsync();

        var result = await controller.SetEinddatum(ov.Id, new SetEinddatumDto());

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);

        var updated = ok!.Value as Opstalverzekering;
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.EindDatum, Is.Not.Null);

        var inDb = await db.Opstalverzekeringen.FindAsync(ov.Id);
        Assert.That(inDb!.EindDatum, Is.Not.Null);
    }

    [Test]
    public async Task SetEinddatum_ReturnsBadRequest_WhenAlreadySet()
    {
        using var db = DbTestUtils.CreateInMemoryDbContext();
        var klant = await SeedActiveKlant(db);
        var controller = CreateController(db);

        var ov = new Opstalverzekering
        {
            PolisNummer = 7007,
            KlantId = klant.Id,
            TypeDekking = "Basis",
            Herbouwwaarde = 100000m,
            Inboedelwaarde = 10000m,
            Premie = 10m,
            Betaaltermijn = "Maandelijks",
            BeginDatum = DateTime.Now,
            EindDatum = DateTime.Now.AddDays(-1)
        };
        db.Opstalverzekeringen.Add(ov);
        await db.SaveChangesAsync();

        var result = await controller.SetEinddatum(ov.Id, new SetEinddatumDto { EindDatum = DateTime.Now });

        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest, Is.Not.Null);
        Assert.That(badRequest!.Value?.ToString(), Does.Contain("al een einddatum"));
    }
}
