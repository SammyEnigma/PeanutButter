﻿using System.Collections.Generic;

namespace RandomBuilderPerformanceTest.Fortel
{
    public class QualityAssurance : EntityBase
    {
        public string QualityAssuranceNumber { get; set; }
        public List<Attachment> Attachments { get; set; }
        public Status Status { get; set; }
        public bool ByPassedQa { get; set; }
        public ElectronicChecklist ElectronicChecklist { get; set; }
    }
}