#!/bin/sh
./Analysis <<EOF
/csv_destination/population.csv
EOF

eval "$@"
