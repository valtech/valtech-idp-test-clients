name := "scala-idp-client"

organization := "valtech"

version := "1.0.0"

scalaVersion := "2.11.2"

libraryDependencies += ws

lazy val root = (project in file(".")).enablePlugins(PlayScala)