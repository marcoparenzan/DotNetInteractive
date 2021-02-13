using System;
using System.IO;
using Microsoft.Spark;
using Microsoft.Spark.Sql;

using static Microsoft.Spark.Sql.Functions;

// Create a Spark session
SparkSession spark = SparkSession
    .Builder()
    .AppName("montecarlo")
    .GetOrCreate();

long rows = 5_000_000_000;

// Create initial DataFrame
DataFrame dataFrame = spark
    .Range(0, rows)
    .Select(Rand().Multiply(2).Minus(1).As("x"), Rand().Multiply(2).Minus(1).As("y"))
    .Select(
        When(
            Col("x").Multiply(Col("x")).Plus(Col("y").Multiply(Col("y"))).Leq(1), 1
        )
        .Otherwise(0)
        .As("InCircle"))
    .Agg(Sum(Col("InCircle")).As("t"))
    .Select(Col("t").Multiply(4).Divide(rows).As("P1"))
    ;

var start = DateTime.Now;

dataFrame.Show();

var stop = DateTime.Now;
Console.WriteLine($"Total Seconds: {(stop-start).TotalSeconds}");

//var result = dataFrame.Head()[0];

//Console.WriteLine($"P1 is about {result}");

// Stop Spark session
spark.Stop();
