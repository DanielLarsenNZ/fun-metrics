
Describe "FunMetrics1 Blob JPEG" {
    It "Returns 200" {
        $result = Invoke-WebRequest -Method GET -Uri "https://funmetrics.azurewebsites.net/api/blob?path=podcasts/s2e01_960.jpg" `
                                    -UseBasicParsing 
        $result.StatusCode | Should -Be 200
    }
}

Describe "FunMetrics1 episode JPEG" {
    It "Returns 200" {
        $result = Invoke-WebRequest -Method GET -Uri "https://funmetrics.azurewebsites.net/episodes/s2e01_960.jpg" `
                                    -UseBasicParsing
        $result.StatusCode | Should -Be 200
    }
}

Describe "FunMetrics2 Blob JPEG" {
    It "Returns 200" {
        $result = Invoke-WebRequest -Method GET -Uri "https://funmetrics2.azurewebsites.net/api/blob?path=podcasts/s2e01_960.jpg" `
                                    -UseBasicParsing 
        $result.StatusCode | Should -Be 200
    }
}

Describe "FunMetrics2 episode JPEG" {
    It "Returns 200" {
        $result = Invoke-WebRequest -Method GET -Uri "https://funmetrics2.azurewebsites.net/episodes/s2e01_960.jpg" `
                                    -UseBasicParsing
        $result.StatusCode | Should -Be 200
    }
}

Describe "FrontDoor Blob JPEG" {
    It "Returns 200" {
        $result = Invoke-WebRequest -Method GET -Uri "https://azurelunch.azurefd.net/api/blob?path=podcasts/s2e01_960.jpg" `
                                    -UseBasicParsing 
        $result.StatusCode | Should -Be 200
    }
}

Describe "FrontDoor episode JPEG" {
    It "Returns 200" {
        $result = Invoke-WebRequest -Method GET -Uri "https://azurelunch.azurefd.net/episodes/s2e01_960.jpg" `
                                    -UseBasicParsing
        $result.StatusCode | Should -Be 200
    }
}
