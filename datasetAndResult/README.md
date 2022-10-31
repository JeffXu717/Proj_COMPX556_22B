## Dataset
- instances1.csv: matrices with n = 100
- instances2.csv: matrices with n = 250
- instances3.csv: matrices with n = 500

## Test result
naming scheme of results:  
- result1_s_dot1.txt  
test result of instance1.csv, serial version ACO, m = 0.1n
- result2_p_dot3.txt  
test result of instance2.csv, parallel version ACO, m = 0.3n

## How to generate result file?
- result1_s_dot1.txt  
Run command: `MMDP sSolve "Filepath\instances1.csv" 0.1 200 20 -a -n 10 -w "Filepath\result1_s_dot1.txt"`
- result2_p_dot3.txt  
Run command: `MMDP pSolve "Filepath\instances2.csv" 0.3 200 10 2 -a -n 10 -w "Filepath\result2_p_dot3.txt"`
