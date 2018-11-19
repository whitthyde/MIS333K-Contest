﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace fa18Team22.Models
{
<<<<<<< HEAD:fa18Team22/fa18Team22/Models/AppUser.cs
    public class AppUser : IdentityUser
=======
    public class AppUser
>>>>>>> 1bf2eabac66a7b6b6ea7387fb641fa68e0c97bbf:fa18Team22/fa18Team22/Models/User.cs
    {
        //[Key]
        //[DataType(DataType.EmailAddress)]
        //[Required(ErrorMessage = "Email address is required")]
        //[Display(Name = "Email Address")]
        //public String EmailAddress { get; set; }

        //[DataType(DataType.PhoneNumber)]
        //[Required(ErrorMessage = "Phone number is required")]
        //[Display(Name = "Phone Number")]
        //public String PhoneNumber { get; set; }


        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public String FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public String LastName { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [Display(Name = "Address")]
        public String Address { get; set; }

        [Required(ErrorMessage = "City is required")]
        [Display(Name = "City")]
        public String City { get; set; }

        [Required(ErrorMessage = "Zip is required")]
        [StringLength(5, ErrorMessage = "Zip is only 5 numbers")]
        [Display(Name = "Zip")]
        public String Zip { get; set; }

        //[Required(ErrorMessage = "Email Address is required")]
        //[StringLength(15, ErrorMessage = "Max of 15 characters for password")]
        //public String Password { get; set; }

        [DataType(DataType.CreditCard)]
        public String CreditCard1 { get; set; }

        [DataType(DataType.CreditCard)]
        public String CreditCard2 { get; set; }

        [DataType(DataType.CreditCard)]
        public String CreditCard3 { get; set; }



        //navigational properties
        public List<Order> Orders { get; set; }
        public List<Review> ReviewsWritten { get; set; }
        public List<Review> ReviewsApproved { get; set; }
    }
}
