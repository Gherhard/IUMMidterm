"use strict";

var Simulator = {
    canvas : undefined,
    canvasContext : undefined,
    rectanglePosition : 0
};

Simulator.start = function () {
    Simulator.canvas = document.getElementById("myCanvas");
    Simulator.canvasContext = Simulator.canvas.getContext("2d");
    Simulator.draw();
};

document.addEventListener( 'DOMContentLoaded', Simulator.start);

Simulator.clearCanvas = function () {
    Simulator.canvasContext.clearRect(0, 0, Simulator.canvas.width, Simulator.canvas.height);
};

Simulator.draw = function () {
    
    $(document).on('click','#btn-draw',function(){

    var xi0 = parseInt($('#x0').val());
    var xi1 = parseInt($('#x1').val());
    var yi0 = parseInt($('#y0').val());
    var yi1 = parseInt($('#y1').val());

    Simulator.clearCanvas();
    drawGrid(); 
    DrawLineBresenham(xi0,xi1,yi0,yi1); 
    DrawLineDDA(xi0,xi1,yi0,yi1);
    });
    
    $(document).on('click','#btn-reset',function(){
        Simulator.clearCanvas();
        document.getElementById('x0').value='';
        document.getElementById('y0').value='';
        document.getElementById('x1').value='';
        document.getElementById('y1').value='';
    });
    
};

function DrawLineBresenham(x0, x1, y0, y1){
  var x1, y1, x2, y2, x, y, dx, dy, xend, p, duady, duadydx;
  x = x0;
  y = y0;

  dx = Math.abs(x1 - x0);
  dy = Math.abs(y1 - y0);

  p = 2 * dy - dx;
  duady = 2 * dy;
  duadydx = 2 * (dy - dx);

  if (x0 > x1) {
    x = x1;
    y = y1;
    xend = x0;
  }
  else
  {
    x = x0;
    y = y0;
    xend = x1;
  }
  drawpixel(x,y,0);

  while (x < xend) {
    x++;
    if (p < 0) {
      p += duady;
    }
    else
    {
      if (y0 > y1) {
        y--;
      }else 
        y++;
      p += duadydx;
    }
    drawpixel(x,y,0);
  }
}

function DrawLineDDA(x0, x1, y0, y1){
    var x, y,dx,dy,steps,xi,yi;
    x = x0;
    y = y0;
    
    dx = x1-x0;
    dy = y1-y0;
    
    if(dx > dy) {
        steps = dx;
    }
    else
        steps = dy;
    
    xi = dx / steps;
    yi = dy / steps;
    
    drawpixel(x,y,1);
    
    do {
    x += xi;
    y += yi;
    drawpixel(x,y,1);
  } while (x < x1);
    		
}
function drawpixel(x,y,color){
   Simulator.canvasContext.beginPath();
   Simulator.canvasContext.moveTo(x,y);
   Simulator.canvasContext.lineTo(x+1,y+1);
   if(color == 0){
       Simulator.canvasContext.strokeStyle= 'yellow';
   }
   else 
        Simulator.canvasContext.strokeStyle= 'blue';
    
   Simulator.canvasContext.stroke();    
}

function drawGrid(){
    var p = 0;
    for (var x = 0; x <= 800; x += 10) {
        Simulator.canvasContext.moveTo(x, 0);
        Simulator.canvasContext.lineTo(x + p, 600 + p);
    }


    for (var x = 0; x <= 600; x += 10) {
        Simulator.canvasContext.moveTo(p, x + p);
        Simulator.canvasContext.lineTo(800 + p,x + p);
    }

    Simulator.canvasContext.strokeStyle = "#AAA";
    Simulator.canvasContext.stroke();
  
}

