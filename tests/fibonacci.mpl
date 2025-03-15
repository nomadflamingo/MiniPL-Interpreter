var n1 : int := 0;
var n2 : int := 1;
var count : int;

var swap : int;

// print first 10 fib numbers
for count in 0..10 do
	print n1;
	print "\n";
	swap := n1 + n2;
	n1 := n2;
	n2 := swap;
end for;
