using Microsoft.AspNetCore.Mvc;
using BallisticCalculator;
using Gehtsoft.Measurements;
using TodoApi.Models; 

namespace TodoApi.Controllers;

[ApiController]
[Route("calculate_shot")]
public class CalculatorController : ControllerBase
{

[HttpPost(Name = "calculate_shot")]
public ShotResult Post(Shot shot)
{
    var ammo = new Ammunition(
        weight: new Measurement<WeightUnit>(shot.ammo.weight, WeightUnit.Gram),
        muzzleVelocity: new Measurement<VelocityUnit>(shot.ammo.muzzleVelocity, VelocityUnit.MetersPerSecond),
        ballisticCoefficient: new BallisticCoefficient(shot.ammo.ballisticCoefficient, DragTableId.G1),
        bulletDiameter: new Measurement<DistanceUnit>(shot.ammo.bulletDiameter, DistanceUnit.Millimeter),
        bulletLength: new Measurement<DistanceUnit>(shot.ammo.bulletLength, DistanceUnit.Millimeter));

    var sight = new Sight(
        sightHeight: new Measurement<DistanceUnit>(shot.ammo.sightHeight, DistanceUnit.Centimeter),
        verticalClick: new Measurement<AngularUnit>(0, AngularUnit.InchesPer100Yards),
        horizontalClick: new Measurement<AngularUnit>(0, AngularUnit.InchesPer100Yards));

    var rifling = new Rifling(
        riflingStep: new Measurement<DistanceUnit>(shot.ammo.riflingStep, DistanceUnit.Centimeter),
        direction: TwistDirection.Right);

    var zero = new ZeroingParameters(
        distance: new Measurement<DistanceUnit>(shot.ammo.zeroDistance, DistanceUnit.Meter),
        ammunition: null,
        atmosphere: null);

    var rifle = new Rifle(sight: sight, zero: zero, rifling: rifling);

    var atmosphere = new Atmosphere(
        pressure: new Measurement<PressureUnit>(shot.scenary.pressure, PressureUnit.MillimetersOfMercury),
        pressureAtSeaLevel: true,
        altitude: new Measurement<DistanceUnit>(shot.scenary.altitude, DistanceUnit.Meter),
        temperature: new Measurement<TemperatureUnit>(shot.scenary.temperature, TemperatureUnit.Celsius),
        humidity: shot.scenary.humidity);

    var calc = new TrajectoryCalculator();

    var shotParams = new ShotParameters()
    {
        MaximumDistance = new Measurement<DistanceUnit>(shot.target_distance, DistanceUnit.Meter),
        Step = new Measurement<DistanceUnit>(10, DistanceUnit.Meter),
        SightAngle = calc.SightAngle(ammo, rifle, atmosphere),
        ShotAngle = new Measurement<AngularUnit>(shot.angle, AngularUnit.Degree),
    };

    Wind[] wind = new Wind[1]
    {
        new Wind()
        {
            Direction = new Measurement<AngularUnit>(shot.scenary.windDirection, AngularUnit.Degree),
            Velocity = new Measurement<VelocityUnit>(shot.scenary.windVelocity, VelocityUnit.KilometersPerHour),
        },
    };

    var trajectory = calc.Calculate(ammo, rifle, atmosphere, shotParams, wind);
    
    var lastPoint = trajectory.Last();
    return new ShotResult
    {
    x = shot.x + (float) lastPoint.Drop.In(DistanceUnit.Inch),
    y = shot.y + (float) lastPoint.Windage.In(DistanceUnit.Inch)

    };
}

}
