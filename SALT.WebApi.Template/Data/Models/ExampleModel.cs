﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SALT.WebApi.Template.Data.Models;

/// <summary>
/// Template for STI report
/// </summary>
[Table("example")]
public class ExampleModel
{
    /// <summary>
    /// id (autogenerated by DB)
    /// </summary>
    [Column("id", TypeName = "serial")]
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// name
    /// </summary>
    [Column("name", TypeName = "varchar(100)")]
    public string Name { get; set; }

    /// <summary>
    /// body
    /// </summary>
    [Column("bytes", TypeName = "bytea")]
    public string Bytes { get; set; }
}