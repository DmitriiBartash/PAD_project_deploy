﻿namespace Manager_App.Models
{
    public class BTUCalculatorViewModel
    {
        public BTURequestModel RequestModel { get; set; }
        public BTUResponseModel ResponseModel { get; set; }
        public List<ConditionerModel> ConditionerModels { get; set; }
    }
}
