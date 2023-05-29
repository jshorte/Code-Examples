#include <iostream>
#include <SDL.h>
#include <gl/glew.h>
#include <SDL_opengl.h>
#include "Shader.h"
#include "main.h"

#include <glm/glm.hpp>
#include <glm/gtc/matrix_transform.hpp>
#include <glm/gtc/type_ptr.hpp>

#pragma region Functions
#pragma endregion

int main(int argc, char** argsv)
{
	//Initialises the SDL Library, passing in SDL_INIT_VIDEO to only initialise the video subsystems
	//NOTE: Using SDL_InitSubSystem() over SDL_Init() for readability 
	//https://wiki.libsdl.org/SDL_Init
	if (SDL_InitSubSystem(SDL_INIT_VIDEO) < 0) 
	{
		//Display an error message box
		//https://wiki.libsdl.org/SDL_ShowSimpleMessageBox
		SDL_ShowSimpleMessageBox(SDL_MESSAGEBOX_ERROR, "SDL_Init failed", SDL_GetError(), NULL);
		return 1;
	}

	SDL_GL_SetAttribute(SDL_GL_CONTEXT_MAJOR_VERSION, 3);
	SDL_GL_SetAttribute(SDL_GL_CONTEXT_MAJOR_VERSION, 2);
	SDL_GL_SetAttribute(SDL_GL_CONTEXT_PROFILE_MASK, SDL_GL_CONTEXT_PROFILE_CORE);

	//Create a window, note we have to free the pointer returned using the DestroyWindow Function
	//https://wiki.libsdl.org/SDL_CreateWindow
	SDL_Window* window = SDL_CreateWindow("SDL2 Window", SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, 960, 720, SDL_WINDOW_OPENGL | SDL_WINDOW_SHOWN);
	//Checks to see if the window has been created, the pointer will have a value of some kind
	if (window == nullptr)
	{
		//Show error
		SDL_ShowSimpleMessageBox(SDL_MESSAGEBOX_ERROR, "SDL_CreateWindow failed", SDL_GetError(), NULL);
		//Close the SDL Library
		//https://wiki.libsdl.org/SDL_Quit
		SDL_Quit();
		return 1;
	}

	SDL_SetRelativeMouseMode(SDL_TRUE);

	//Make the context of our window the main context on the current thread
	SDL_GLContext glContext = SDL_GL_CreateContext(window); 

	glEnable(GL_DEPTH_TEST);

	glewExperimental = GL_TRUE;
	GLenum glewError = glewInit();
	if (glewError != GLEW_OK)
	{
		SDL_ShowSimpleMessageBox(SDL_MESSAGEBOX_ERROR, "Unable to initialise GLEW", (char*)glewGetErrorString(glewError), NULL);
	}

	//An array of Verticies containing Position and Color data
	static const GLfloat vertices[] =
	{
		//Positions				    //Colors				
		-0.5f , -0.5f, 0.0f,		1.0f, 0.0f, 0.0f, //Front Bottom Left	0
		 0.5f,  -0.5f, 0.0f, 		0.0f, 0.5f, 0.0f, //Front Bottom Right  1
		 0.5f,   0.5f, 0.0f,		0.0f, 0.0f, 1.0f, //Front Top Right		2	
		-0.5f,   0.5f, 0.0f,		0.0f, 0.5f, 0.0f, //Front Top Left		3

		-0.5f , -0.5f, -1.0f,		1.0f, 0.0f, 0.0f, //Back Bottom Left    4
		-0.5f ,  0.5f, -1.0f,		0.0f, 0.5f, 0.0f, //Back Top Left       5

		 0.5f , -0.5f, -1.0f,		0.0f, 0.5f, 0.0f, //Back Bottom Right   6
		 0.5f ,  0.5f, -1.0f,		0.0f, 0.0f, 1.0f  //Back Top Right      7
	};
	static const GLuint indices[] =
	{		
		//front
		0,1,3, //BL, BR, TL
		1,2,3, //BR, TR, TL
		//left
		4,0,3, //BBL, FBL, FTL
		3,5,4, //FTL, BTL, BBL
		//right
		1,6,7, //FBR, BBR, BTR
		7,2,1, //BTR, FTR, FBR
		//back
		4,5,6,
		5,7,6,
		//top
		2,3,5,
		5,7,2,
		//bottom
		0,1,6,
		6,4,0
	};

	//Create vertex array object (VAO)
	GLuint VAO;
	//Generate VAO(s)
	glGenVertexArrays(1, &VAO); //First param is the number of object(s), Second is object
	//Bind gl calls to VAO
	glBindVertexArray(VAO);

	//Storage area for buffer data
	GLuint VBO;
	//Generate one buffer, put the resulting identifier in VBO
	glGenBuffers(1, &VBO);
	//Bind the buffer so the operations of GLBuffer apply to the VBO
	glBindBuffer(GL_ARRAY_BUFFER, VBO);

	//Set the current buffer target data to the VBO
	//Static draw means we are making a static object - we define the object once and do not change it 
	glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);

	//Storage area for buffer data
	GLuint EBO;
	//Generate one buffer, put the resulting identifier in EBO
	glGenBuffers(1, &EBO);
	//Bind the buffer so the operations of GLBuffer apply to the EBO
	glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, EBO);

	glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(indices), indices, GL_STATIC_DRAW);

	//Create and compile our GLSL program from the shaders
	GLuint programID = LoadShaders("BasicVert.glsl", "BasicFrag.glsl");

	SDL_GL_SwapWindow(window);

	//For the Positions in the buffer
	glEnableVertexAttribArray(0);
	glVertexAttribPointer(
		0,								//Index (Specifies the index of the generic vertex attribute to be modified)
		3,								//Size (Specifies the number of components per generic vertex attribute)
		GL_FLOAT,						//Type (Specifies the data type of each component in the array)
		GL_FALSE,						//Normalisation (specifies whether fixed-point data values should be normalized (GL_TRUE) or converted directly as fixed-point values (GL_FALSE))
		6 * sizeof(GL_FLOAT),			//Stride specifies the byte offset between consecutive generic vertex attributes (Number of elements between attributes * data size (Bytes)
		(void*)0						//Pointer (Specifies a offset of the first component of the first generic vertex attribute in the array)
	);
	//For the Colors in the buffer
	glEnableVertexAttribArray(1);
	glVertexAttribPointer(
		1,								//Index (Specifies the index of the generic vertex attribute to be modified)
		3,								//Size (Specifies the number of components per generic vertex attribute)
		GL_FLOAT,						//Type (Specifies the data type of each component in the array)
		GL_FALSE,						//Normalisation (specifies whether fixed-point data values should be normalized (GL_TRUE) or converted directly as fixed-point values (GL_FALSE))
		6 * sizeof(GL_FLOAT),			//Stride specifies the byte offset between consecutive generic vertex attributes (Number of elements between attributes * data size (Bytes)
		(void*)(3 * sizeof(GL_FLOAT))	//Pointer (Specifies a offset of the first component of the first generic vertex attribute in the array)
	);
	
	glm::mat4 model = glm::mat4(1.0f);
	model = glm::rotate(model, glm::radians(0.0f), glm::vec3(0.0, 0.0, 1.0));
	model = glm::scale(model, glm::vec3(0.5, 0.5, 0.5));

	glm::mat4 mvp, view, projection;

	glm::vec3 position(0, 0, 2), forward(0, 0, -1), left(0), rotation(0);
	const glm::vec4 cameraFace(0, 0, -1, 0);
	const glm::vec4 cameraPerp(-1, 0, 0, 0);
	const float walkSpeed = 0.2f, rotSpeed = 0.1f;

	//Gets the uniform "transform" from the shader (programID)
	unsigned int transformLoc = glGetUniformLocation(programID, "transform");

	//Event loop, we will loop until running is set to false, usually if escape has been pressed or window is closed
	bool running = true;
	//SDL Event structure, this will be checked in the while loop
	SDL_Event ev;

	while (running)
	{
		//Poll for the events which have happened in this frame
		//https://wiki.libsdl.org/SDL_PollEvent
		while (SDL_PollEvent(&ev))
		{
			//Switch case for every message we are intereted in
			switch (ev.type)
			{
				//QUIT Message, usually called when the window has been closed
			case SDL_QUIT:
				running = false;
				break;
			case SDL_MOUSEMOTION:
			{
				rotation.y -= ev.motion.xrel * rotSpeed; //anti-clockwise rotation around the X axis
				rotation.x -= ev.motion.yrel * rotSpeed; //anti-clockwise rotation around the Y axis
				glm::mat4 viewRotate(1.0f);
				viewRotate = glm::rotate(viewRotate, glm::radians(rotation.x), glm::vec3(1.0f, 0.0f, 0.0f));
				viewRotate = glm::rotate(viewRotate, glm::radians(rotation.y), glm::vec3(0.0f, 1.0f, 0.0f));
				forward = glm::normalize(glm::vec3(viewRotate * cameraFace));				
				left = glm::normalize(glm::vec3(viewRotate * cameraPerp));
				break;
			}			
			//KEYDOWN Message, called when a key has been pressed down
			case SDL_KEYDOWN:
				//Check the actual key code of the key that has been pressed
				switch (ev.key.keysym.sym)
				{
					//Escape key
				case SDLK_ESCAPE:
					running = false;
					break;
				case SDLK_w:
					position += walkSpeed * forward;
					break;
				case SDLK_s:
					position -= walkSpeed * forward;
					break;
				case SDLK_a:
					position += walkSpeed * left;
					break;
				case SDLK_d:
					position -= walkSpeed * left;
					break;
				}
			}
		}

		glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
		//Clear the screen (prevents drawing over previous screen)
		glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

		glUseProgram(programID);
		//Position and direction of the camera
		view = glm::lookAt(position, position + forward, glm::vec3(0, 1, 0));
		//FoV, aspect ratio + clipping plane of the camera (perspective)
		projection = glm::perspective(glm::radians(45.0f), 4.0f / 3.0f, 0.1f, 100.0f);
		//Noncommutative (column major order)
		mvp = projection * view * model;
		glUniformMatrix4fv(transformLoc, 1, GL_FALSE, glm::value_ptr(mvp));

		glDrawElements(GL_TRIANGLES, 36, GL_UNSIGNED_INT, (void*)0);

		SDL_GL_SwapWindow(window);
		
	}

	//Destroy the window and quit SDL2, NB we should do this after all cleanup in this order!!!
	//https://wiki.libsdl.org/SDL_DestroyWindow
	SDL_DestroyWindow(window);
	//https://wiki.libsdl.org/SDL_Quit
	SDL_Quit();

	return 0;
}